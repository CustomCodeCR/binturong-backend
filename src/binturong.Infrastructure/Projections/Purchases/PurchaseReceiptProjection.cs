using Application.Abstractions.Projections;
using Application.ReadModels.Purchases;
using Domain.PurchaseReceipts;
using MongoDB.Driver;

namespace Infrastructure.Projections.Purchases;

internal sealed class PurchaseReceiptProjection
    : IProjector<PurchaseReceiptCreatedDomainEvent>,
        IProjector<PurchaseReceiptLineAddedDomainEvent>,
        IProjector<PurchaseReceiptRejectedDomainEvent>
{
    private const string CollectionName = "purchase_receipts";
    private readonly IMongoDatabase _db;

    public PurchaseReceiptProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(PurchaseReceiptCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PurchaseReceiptReadModel>(CollectionName);

        var doc = new PurchaseReceiptReadModel
        {
            Id = $"purchase_receipt:{e.ReceiptId}",
            ReceiptId = e.ReceiptId,

            PurchaseOrderId = e.PurchaseOrderId,
            PurchaseOrderCode = string.Empty, // TODO resolve from purchase_orders read model

            WarehouseId = e.WarehouseId,
            WarehouseName = string.Empty, // TODO resolve from warehouses

            Status = e.Status,
            Notes = e.Notes,

            Lines = new List<PurchaseReceiptLineReadModel>(),
        };

        var exists = await col.Find(x => x.ReceiptId == e.ReceiptId).AnyAsync(ct);
        if (exists)
            return;

        await col.InsertOneAsync(doc, new InsertOneOptions(), ct);
    }

    public async Task ProjectAsync(PurchaseReceiptLineAddedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PurchaseReceiptReadModel>(CollectionName);

        var update = Builders<PurchaseReceiptReadModel>.Update.Push(
            x => x.Lines,
            new PurchaseReceiptLineReadModel
            {
                ReceiptDetailId = e.ReceiptDetailId,
                ProductId = e.ProductId,
                ProductName = string.Empty, // TODO resolve from products read model
                QuantityReceived = e.QuantityReceived,
                UnitCost = e.UnitCost,
            }
        );

        await col.UpdateOneAsync(
            x => x.ReceiptId == e.ReceiptId,
            update,
            new UpdateOptions { IsUpsert = false },
            ct
        );
    }

    public async Task ProjectAsync(PurchaseReceiptRejectedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PurchaseReceiptReadModel>(CollectionName);

        var update = Builders<PurchaseReceiptReadModel>
            .Update.Set(x => x.Status, e.Status)
            .Set(x => x.Notes, $"Rejected: {e.Reason}");

        await col.UpdateOneAsync(
            x => x.ReceiptId == e.ReceiptId,
            update,
            new UpdateOptions { IsUpsert = false },
            ct
        );
    }
}
