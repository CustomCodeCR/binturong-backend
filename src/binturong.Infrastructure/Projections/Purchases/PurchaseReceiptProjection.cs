using Application.Abstractions.Projections;
using Application.ReadModels.Inventory;
using Application.ReadModels.MasterData;
using Application.ReadModels.Purchases;
using Domain.PurchaseReceipts;
using MongoDB.Driver;

namespace Infrastructure.Projections.Purchases;

internal sealed class PurchaseReceiptProjection
    : IProjector<PurchaseReceiptCreatedDomainEvent>,
        IProjector<PurchaseReceiptLineAddedDomainEvent>,
        IProjector<PurchaseReceiptRejectedDomainEvent>
{
    private const string ReceiptsCol = "purchase_receipts";
    private const string PurchaseOrdersCol = "purchase_orders";
    private const string WarehousesCol = "warehouses";
    private const string ProductsCol = "products";

    private readonly IMongoDatabase _db;

    public PurchaseReceiptProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(PurchaseReceiptCreatedDomainEvent e, CancellationToken ct)
    {
        var receipts = _db.GetCollection<PurchaseReceiptReadModel>(ReceiptsCol);

        var purchaseOrderCode = await GetPurchaseOrderCodeAsync(e.PurchaseOrderId, ct);
        var warehouseName = await GetWarehouseNameAsync(e.WarehouseId, ct);

        var id = $"purchase_receipt:{e.ReceiptId}";
        var filter = Builders<PurchaseReceiptReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<PurchaseReceiptReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .Set(x => x.ReceiptId, e.ReceiptId)
            .Set(x => x.PurchaseOrderId, e.PurchaseOrderId)
            .Set(x => x.PurchaseOrderCode, purchaseOrderCode)
            .Set(x => x.WarehouseId, e.WarehouseId)
            .Set(x => x.WarehouseName, warehouseName)
            .Set(x => x.Status, e.Status)
            .Set(x => x.Notes, e.Notes)
            .SetOnInsert(x => x.Lines, new List<PurchaseReceiptLineReadModel>());

        await receipts.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(PurchaseReceiptLineAddedDomainEvent e, CancellationToken ct)
    {
        var receipts = _db.GetCollection<PurchaseReceiptReadModel>(ReceiptsCol);

        var id = $"purchase_receipt:{e.ReceiptId}";
        var filter = Builders<PurchaseReceiptReadModel>.Filter.Eq(x => x.Id, id);

        var productName = await GetProductNameAsync(e.ProductId, ct);

        var line = new PurchaseReceiptLineReadModel
        {
            ReceiptDetailId = e.ReceiptDetailId,
            ProductId = e.ProductId,
            ProductName = productName,
            QuantityReceived = e.QuantityReceived,
            UnitCost = e.UnitCost,
        };

        await receipts.UpdateOneAsync(
            filter,
            Builders<PurchaseReceiptReadModel>
                .Update.SetOnInsert(x => x.Id, id)
                .SetOnInsert(x => x.ReceiptId, e.ReceiptId)
                .SetOnInsert(x => x.Lines, new List<PurchaseReceiptLineReadModel>()),
            new UpdateOptions { IsUpsert = true },
            ct
        );

        await receipts.UpdateOneAsync(
            filter,
            Builders<PurchaseReceiptReadModel>.Update.Push(x => x.Lines, line),
            new UpdateOptions { IsUpsert = false },
            ct
        );
    }

    public async Task ProjectAsync(PurchaseReceiptRejectedDomainEvent e, CancellationToken ct)
    {
        var receipts = _db.GetCollection<PurchaseReceiptReadModel>(ReceiptsCol);

        var id = $"purchase_receipt:{e.ReceiptId}";

        var update = Builders<PurchaseReceiptReadModel>
            .Update.Set(x => x.Status, e.Status)
            .Set(x => x.Notes, $"Rejected: {e.Reason}");

        await receipts.UpdateOneAsync(
            x => x.Id == id,
            update,
            new UpdateOptions { IsUpsert = false },
            ct
        );
    }

    private async Task<string> GetPurchaseOrderCodeAsync(Guid purchaseOrderId, CancellationToken ct)
    {
        var purchaseOrders = _db.GetCollection<PurchaseOrderReadModel>(PurchaseOrdersCol);
        var purchaseOrderDocId = $"purchase_order:{purchaseOrderId}";

        var purchaseOrder = await purchaseOrders
            .Find(x => x.Id == purchaseOrderDocId)
            .FirstOrDefaultAsync(ct);

        return purchaseOrder?.Code ?? string.Empty;
    }

    private async Task<string> GetWarehouseNameAsync(Guid warehouseId, CancellationToken ct)
    {
        var warehouses = _db.GetCollection<WarehouseReadModel>(WarehousesCol);
        var warehouseDocId = $"warehouse:{warehouseId}";

        var warehouse = await warehouses.Find(x => x.Id == warehouseDocId).FirstOrDefaultAsync(ct);

        return warehouse?.Name ?? string.Empty;
    }

    private async Task<string> GetProductNameAsync(Guid productId, CancellationToken ct)
    {
        var products = _db.GetCollection<ProductReadModel>(ProductsCol);
        var productDocId = $"product:{productId}";

        var product = await products.Find(x => x.Id == productDocId).FirstOrDefaultAsync(ct);

        return product?.Name ?? string.Empty;
    }
}
