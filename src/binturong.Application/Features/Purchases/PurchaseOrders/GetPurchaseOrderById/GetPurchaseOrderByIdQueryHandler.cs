using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Purchases;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Purchases.PurchaseOrders.GetPurchaseOrderById;

internal sealed class GetPurchaseOrderByIdQueryHandler
    : IQueryHandler<GetPurchaseOrderByIdQuery, PurchaseOrderReadModel>
{
    private const string CollectionName = "purchase_orders";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPurchaseOrderByIdQueryHandler(
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

    public async Task<Result<PurchaseOrderReadModel>> Handle(
        GetPurchaseOrderByIdQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PurchaseOrderReadModel>(CollectionName);

        var doc = await col.Find(x => x.PurchaseOrderId == q.PurchaseOrderId)
            .FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Purchases",
            "PurchaseOrder",
            q.PurchaseOrderId,
            "PURCHASE_ORDER_READ",
            string.Empty,
            $"purchaseOrderId={q.PurchaseOrderId}; found={(doc is not null)}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<PurchaseOrderReadModel>(
                Error.NotFound(
                    "PurchaseOrders.NotFound",
                    $"Purchase order '{q.PurchaseOrderId}' not found"
                )
            )
            : Result.Success(doc);
    }
}
