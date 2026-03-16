using System.Text.RegularExpressions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Common.Selects;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.Purchases;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Purchases.Requests.GetPurchaseRequestsSelect;

internal sealed class GetPurchaseRequestsSelectQueryHandler
    : IQueryHandler<GetPurchaseRequestsSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "PurchaseRequests";
    private const string Entity = "PurchaseRequest";
    private const int MaxSelectResults = 100;

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPurchaseRequestsSelectQueryHandler(
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
        GetPurchaseRequestsSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PurchaseRequestReadModel>(MongoCollections.PurchaseRequests);

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.RequestDate)
            .ThenBy(x => x.Code)
            .Limit(MaxSelectResults)
            .ToListAsync(ct);

        var result = docs.Select(x => new SelectOptionDto(
                x.RequestId.ToString(),
                BuildLabel(x),
                BuildCode(x)
            ))
            .ToList();

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "PURCHASE_REQUESTS_SELECT_READ",
                string.Empty,
                $"search={normalizedSearch ?? ""}; limit={MaxSelectResults}; returned={result.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(result);
    }

    private static FilterDefinition<PurchaseRequestReadModel> BuildFilter(string? search)
    {
        var builder = Builders<PurchaseRequestReadModel>.Filter;

        if (string.IsNullOrWhiteSpace(search))
        {
            return builder.Empty;
        }

        var escaped = Regex.Escape(search);
        var startsWithRegex = new BsonRegularExpression($"^{escaped}", "i");
        var containsRegex = new BsonRegularExpression(escaped, "i");

        return builder.Or(
            builder.Regex(x => x.Code, startsWithRegex),
            builder.Regex(x => x.Status, containsRegex),
            builder.Regex(x => x.BranchName, containsRegex),
            builder.Regex(x => x.RequestedByName, containsRegex),
            builder.Regex(x => x.Notes, containsRegex)
        );
    }

    private static string BuildLabel(PurchaseRequestReadModel x)
    {
        var code = x.Code?.Trim();
        var branchName = x.BranchName?.Trim();
        var requestedByName = x.RequestedByName?.Trim();

        if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(branchName))
        {
            return $"{code} - {branchName}";
        }

        if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(requestedByName))
        {
            return $"{code} - {requestedByName}";
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            return code;
        }

        return x.RequestId.ToString();
    }

    private static string? BuildCode(PurchaseRequestReadModel x)
    {
        if (!string.IsNullOrWhiteSpace(x.Code))
            return x.Code;

        return x.Status;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
