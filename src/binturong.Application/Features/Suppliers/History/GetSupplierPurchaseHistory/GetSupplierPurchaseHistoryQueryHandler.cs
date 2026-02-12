using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Purchases;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Suppliers.History.GetSupplierPurchaseHistory;

internal sealed class GetSupplierPurchaseHistoryQueryHandler
    : IQueryHandler<GetSupplierPurchaseHistoryQuery, IReadOnlyList<PurchaseOrderReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetSupplierPurchaseHistoryQueryHandler(
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
        GetSupplierPurchaseHistoryQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PurchaseOrderReadModel>(MongoCollections.PurchaseOrders);

        var filter = Builders<PurchaseOrderReadModel>.Filter.Eq(
            x => x.SupplierId,
            query.SupplierId
        );

        if (query.From is not null)
            filter &= Builders<PurchaseOrderReadModel>.Filter.Gte(
                x => x.OrderDate,
                query.From.Value
            );

        if (query.To is not null)
            filter &= Builders<PurchaseOrderReadModel>.Filter.Lte(x => x.OrderDate, query.To.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            filter &= Builders<PurchaseOrderReadModel>.Filter.Eq(
                x => x.Status,
                query.Status.Trim()
            );

        var docs = await col.Find(filter)
            .SortByDescending(x => x.OrderDate)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Suppliers",
            "SupplierPurchaseHistory",
            query.SupplierId,
            "SUPPLIER_PURCHASE_HISTORY_READ",
            string.Empty,
            $"supplierId={query.SupplierId}; from={query.From}; to={query.To}; status={query.Status}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<PurchaseOrderReadModel>>(docs);
    }
}
