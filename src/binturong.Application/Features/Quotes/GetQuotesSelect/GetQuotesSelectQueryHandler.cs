using System.Text.RegularExpressions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Common.Selects;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Quotes.GetQuotesSelect;

internal sealed class GetQuotesSelectQueryHandler
    : IQueryHandler<GetQuotesSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "Sales";
    private const string Entity = "Quote";
    private const int MaxSelectResults = 100;

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetQuotesSelectQueryHandler(
        IMongoDatabase db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<SelectOptionDto>>> Handle(
        GetQuotesSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<QuoteReadModel>(MongoCollections.Quotes);

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.IssueDate)
            .ThenBy(x => x.Code)
            .Limit(MaxSelectResults)
            .ToListAsync(ct);

        var result = docs.Select(x => new SelectOptionDto(
                x.QuoteId.ToString(),
                BuildLabel(x),
                x.Code
            ))
            .ToList();

        await _bus.AuditAsync(
            _currentUser.UserId,
            Module,
            Entity,
            null,
            "QUOTE_SELECT_READ",
            string.Empty,
            $"search={normalizedSearch ?? ""}; limit={MaxSelectResults}; returned={result.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(result);
    }

    private static FilterDefinition<QuoteReadModel> BuildFilter(string? search)
    {
        var builder = Builders<QuoteReadModel>.Filter;

        if (string.IsNullOrWhiteSpace(search))
            return builder.Empty;

        var escaped = Regex.Escape(search);
        var startsWithRegex = new BsonRegularExpression($"^{escaped}", "i");
        var containsRegex = new BsonRegularExpression(escaped, "i");

        return builder.Or(
            builder.Regex(x => x.Code, startsWithRegex),
            builder.Regex(x => x.ClientName, containsRegex),
            builder.Regex(x => x.BranchName, containsRegex),
            builder.Regex(x => x.Status, containsRegex)
        );
    }

    private static string BuildLabel(QuoteReadModel x)
    {
        var code = x.Code?.Trim();
        var clientName = x.ClientName?.Trim();
        var status = x.Status?.Trim();

        if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(clientName))
        {
            if (!string.IsNullOrWhiteSpace(status))
                return $"{code} - {clientName} ({status})";

            return $"{code} - {clientName}";
        }

        if (!string.IsNullOrWhiteSpace(code))
            return code;

        if (!string.IsNullOrWhiteSpace(clientName))
            return clientName;

        return x.QuoteId.ToString();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
