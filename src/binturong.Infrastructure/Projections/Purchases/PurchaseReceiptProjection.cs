using Application.Abstractions.Projections;
using Application.ReadModels.Purchases;
using Domain.PurchaseReceipts;
using MongoDB.Driver;

namespace Infrastructure.Projections.Purchases;

internal sealed class PurchaseReceiptProjection
    : IProjector<PurchaseReceiptRegisteredDomainEvent>,
        IProjector<PurchaseReceiptDetailAddedDomainEvent>,
        IProjector<PurchaseReceiptRejectedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public PurchaseReceiptProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(PurchaseReceiptRegisteredDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PurchaseReceiptReadModel>("purchase_receipts");

        var doc = new PurchaseReceiptReadModel
        {
            Id = $"purchase_receipt:{e.ReceiptId}",
            ReceiptId = e.ReceiptId,
            PurchaseOrderId = e.PurchaseOrderId,
            PurchaseOrderCode = string.Empty, // TODO resolve from purchase_orders
            WarehouseId = e.WarehouseId,
            WarehouseName = string.Empty, // TODO resolve
            ReceiptDate = e.ReceiptDate,
            Status = e.Status,
            Notes = e.Notes,
            Lines = new List<PurchaseReceiptLineReadModel>(),
        };

        await col.InsertOneAsync(doc, new InsertOneOptions(), ct);
    }

    public async Task ProjectAsync(PurchaseReceiptDetailAddedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PurchaseReceiptReadModel>("purchase_receipts");

        var update = Builders<PurchaseReceiptReadModel>.Update.Push(
            x => x.Lines,
            new PurchaseReceiptLineReadModel
            {
                ReceiptDetailId = e.ReceiptDetailId,
                ProductId = e.ProductId,
                ProductName = string.Empty, // TODO resolve
                QuantityReceived = e.QuantityReceived,
                UnitCost = e.UnitCost,
            }
        );

        await col.UpdateOneAsync(
            x => x.ReceiptId == e.ReceiptId,
            update,
            options: null,
            cancellationToken: ct
        );
    }

    public async Task ProjectAsync(PurchaseReceiptRejectedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PurchaseReceiptReadModel>("purchase_receipts");

        await col.UpdateOneAsync(
            x => x.ReceiptId == e.ReceiptId,
            Builders<PurchaseReceiptReadModel>.Update.Set(x => x.Status, "Rejected"),
            options: null,
            cancellationToken: ct
        );
    }
}
