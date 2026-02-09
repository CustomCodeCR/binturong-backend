using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Purchases;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Purchases.PurchaseReceipts.GetPurchaseReceipts;

internal sealed class GetPurchaseReceiptsQueryHandler
    : IQueryHandler<GetPurchaseReceiptsQuery, IReadOnlyList<PurchaseReceiptReadModel>>
{
    private const string CollectionName = "purchase_receipts";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPurchaseReceiptsQueryHandler(
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

    public async Task<Result<IReadOnlyList<PurchaseReceiptReadModel>>> Handle(
        GetPurchaseReceiptsQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PurchaseReceiptReadModel>(CollectionName);

        var filter = BuildFilter(q.Search);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.ReceiptDate)
            .Skip(q.Skip)
            .Limit(q.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Purchases",
            "PurchaseReceipt",
            null,
            "PURCHASE_RECEIPT_LIST_READ",
            string.Empty,
            $"search={q.Search}; skip={q.Skip}; take={q.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<PurchaseReceiptReadModel>>(docs);
    }

    private static FilterDefinition<PurchaseReceiptReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<PurchaseReceiptReadModel>.Filter.Empty;

        var s = search.Trim();
        var rx = new BsonRegularExpression(s, "i");

        return Builders<PurchaseReceiptReadModel>.Filter.Or(
            Builders<PurchaseReceiptReadModel>.Filter.Regex(x => x.PurchaseOrderCode, rx),
            Builders<PurchaseReceiptReadModel>.Filter.Regex(x => x.WarehouseName, rx),
            Builders<PurchaseReceiptReadModel>.Filter.Regex(x => x.Status, rx),
            Builders<PurchaseReceiptReadModel>.Filter.Regex(x => x.Notes, rx)
        );
    }
}
