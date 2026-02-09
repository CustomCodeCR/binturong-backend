using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Purchases;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Purchases.PurchaseReceipts.GetPurchaseReceiptById;

internal sealed class GetPurchaseReceiptByIdQueryHandler
    : IQueryHandler<GetPurchaseReceiptByIdQuery, PurchaseReceiptReadModel>
{
    private const string CollectionName = "purchase_receipts";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPurchaseReceiptByIdQueryHandler(
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

    public async Task<Result<PurchaseReceiptReadModel>> Handle(
        GetPurchaseReceiptByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PurchaseReceiptReadModel>(CollectionName);

        var doc = await col.Find(x => x.ReceiptId == query.ReceiptId).FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Purchases",
            "PurchaseReceipt",
            query.ReceiptId,
            "PURCHASE_RECEIPT_READ",
            string.Empty,
            $"receiptId={query.ReceiptId}; found={(doc is not null)}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<PurchaseReceiptReadModel>(
                Error.NotFound(
                    "PurchaseReceipts.NotFound",
                    $"Purchase receipt '{query.ReceiptId}' not found"
                )
            )
            : Result.Success(doc);
    }
}
