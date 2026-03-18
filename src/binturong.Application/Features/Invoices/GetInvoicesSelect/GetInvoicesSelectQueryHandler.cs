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

namespace Application.Features.Invoices.GetInvoicesSelect;

internal sealed class GetInvoicesSelectQueryHandler
    : IQueryHandler<GetInvoicesSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "Sales";
    private const string Entity = "Invoice";
    private const int MaxSelectResults = 100;

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetInvoicesSelectQueryHandler(
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
        GetInvoicesSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.IssueDate)
            .ThenBy(x => x.Consecutive)
            .Limit(MaxSelectResults)
            .ToListAsync(ct);

        var result = docs.Select(x => new SelectOptionDto(
                x.InvoiceId.ToString(),
                BuildLabel(x),
                x.Consecutive ?? string.Empty
            ))
            .ToList();

        await _bus.AuditAsync(
            _currentUser.UserId,
            Module,
            Entity,
            null,
            "INVOICE_SELECT_READ",
            string.Empty,
            $"search={normalizedSearch ?? ""}; limit={MaxSelectResults}; returned={result.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(result);
    }

    private static FilterDefinition<InvoiceReadModel> BuildFilter(string? search)
    {
        var builder = Builders<InvoiceReadModel>.Filter;

        if (string.IsNullOrWhiteSpace(search))
            return builder.Empty;

        var escaped = Regex.Escape(search);
        var startsWithRegex = new BsonRegularExpression($"^{escaped}", "i");
        var containsRegex = new BsonRegularExpression(escaped, "i");

        return builder.Or(
            builder.Regex(x => x.Consecutive, startsWithRegex),
            builder.Regex(x => x.TaxKey, containsRegex),
            builder.Regex(x => x.ClientName, containsRegex)
        );
    }

    private static string BuildLabel(InvoiceReadModel x)
    {
        var consecutive = x.Consecutive?.Trim();
        var clientName = x.ClientName?.Trim();

        if (!string.IsNullOrWhiteSpace(consecutive) && !string.IsNullOrWhiteSpace(clientName))
            return $"{consecutive} - {clientName}";

        if (!string.IsNullOrWhiteSpace(consecutive))
            return consecutive;

        if (!string.IsNullOrWhiteSpace(clientName))
            return clientName;

        return x.InvoiceId.ToString();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
