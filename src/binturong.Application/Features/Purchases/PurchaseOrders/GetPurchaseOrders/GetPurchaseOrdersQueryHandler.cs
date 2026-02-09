using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Purchases;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Purchases.PurchaseOrders.GetPurchaseOrders;

internal sealed class GetPurchaseOrdersQueryHandler
    : IQueryHandler<GetPurchaseOrdersQuery, IReadOnlyList<PurchaseOrderReadModel>>
{
    private const string CollectionName = "purchase_orders";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPurchaseOrdersQueryHandler(
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

    public async Task<Result<IReadOnlyList<PurchaseOrderReadModel>>> Handle(
        GetPurchaseOrdersQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PurchaseOrderReadModel>(CollectionName);

        var filter = BuildFilter(q.Search);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.OrderDate)
            .Skip(q.Skip)
            .Limit(q.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Purchases",
            "PurchaseOrder",
            null,
            "PURCHASE_ORDER_LIST_READ",
            string.Empty,
            $"search={q.Search}; skip={q.Skip}; take={q.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<PurchaseOrderReadModel>>(docs);
    }

    private static FilterDefinition<PurchaseOrderReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<PurchaseOrderReadModel>.Filter.Empty;

        var s = search.Trim();
        var rx = new BsonRegularExpression(s, "i");

        return Builders<PurchaseOrderReadModel>.Filter.Or(
            Builders<PurchaseOrderReadModel>.Filter.Regex(x => x.Code, rx),
            Builders<PurchaseOrderReadModel>.Filter.Regex(x => x.SupplierName, rx),
            Builders<PurchaseOrderReadModel>.Filter.Regex(x => x.Status, rx),
            Builders<PurchaseOrderReadModel>.Filter.Regex(x => x.Currency, rx),
            Builders<PurchaseOrderReadModel>.Filter.Regex(x => x.BranchName, rx)
        );
    }
}
