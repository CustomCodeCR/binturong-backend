using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Purchases;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Purchases.PurchaseRequests.GetPurchaseRequests;

internal sealed class GetPurchaseRequestsQueryHandler
    : IQueryHandler<GetPurchaseRequestsQuery, IReadOnlyList<PurchaseRequestReadModel>>
{
    private const string CollectionName = "purchase_requests";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPurchaseRequestsQueryHandler(
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

    public async Task<Result<IReadOnlyList<PurchaseRequestReadModel>>> Handle(
        GetPurchaseRequestsQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PurchaseRequestReadModel>(CollectionName);

        var filter = BuildFilter(q.Search);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.RequestDate)
            .Skip(q.Skip)
            .Limit(q.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Purchases",
            "PurchaseRequest",
            null,
            "PURCHASE_REQUEST_LIST_READ",
            string.Empty,
            $"search={q.Search}; skip={q.Skip}; take={q.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<PurchaseRequestReadModel>>(docs);
    }

    private static FilterDefinition<PurchaseRequestReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<PurchaseRequestReadModel>.Filter.Empty;

        var s = search.Trim();
        var rx = new BsonRegularExpression(s, "i");

        return Builders<PurchaseRequestReadModel>.Filter.Or(
            Builders<PurchaseRequestReadModel>.Filter.Regex(x => x.Code, rx),
            Builders<PurchaseRequestReadModel>.Filter.Regex(x => x.Status, rx),
            Builders<PurchaseRequestReadModel>.Filter.Regex(x => x.Notes, rx),
            Builders<PurchaseRequestReadModel>.Filter.Regex(x => x.BranchName, rx),
            Builders<PurchaseRequestReadModel>.Filter.Regex(x => x.RequestedByName, rx)
        );
    }
}
