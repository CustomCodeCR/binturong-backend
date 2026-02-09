using Application.Abstractions.Projections;
using Application.ReadModels.Purchases;
using Domain.PurchaseOrders;
using MongoDB.Driver;

namespace Infrastructure.Projections.Purchases;

internal sealed class PurchaseOrderProjection
    : IProjector<PurchaseOrderCreatedDomainEvent>,
        IProjector<PurchaseOrderLineAddedDomainEvent>,
        IProjector<PurchaseOrderStatusChangedDomainEvent>
{
    private const string OrdersCol = "purchase_orders";
    private readonly IMongoDatabase _db;

    public PurchaseOrderProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(PurchaseOrderCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PurchaseOrderReadModel>(OrdersCol);

        var id = $"purchase_order:{e.PurchaseOrderId}";
        var filter = Builders<PurchaseOrderReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<PurchaseOrderReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .Set(x => x.PurchaseOrderId, e.PurchaseOrderId)
            .Set(x => x.Code, e.Code)
            .Set(x => x.SupplierId, e.SupplierId)
            .Set(x => x.SupplierName, string.Empty)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.BranchName, null)
            .Set(x => x.RequestId, e.RequestId)
            .Set(x => x.OrderDate, e.OrderDate)
            .Set(x => x.Status, e.Status)
            .Set(x => x.Currency, e.Currency)
            .Set(x => x.ExchangeRate, e.ExchangeRate)
            .Set(x => x.Subtotal, e.Subtotal)
            .Set(x => x.Taxes, e.Taxes)
            .Set(x => x.Discounts, e.Discounts)
            .Set(x => x.Total, e.Total)
            .SetOnInsert(x => x.Lines, Array.Empty<PurchaseOrderLineReadModel>());

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(PurchaseOrderLineAddedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PurchaseOrderReadModel>(OrdersCol);

        var id = $"purchase_order:{e.PurchaseOrderId}";

        var update = Builders<PurchaseOrderReadModel>.Update.Push(
            x => x.Lines,
            new PurchaseOrderLineReadModel
            {
                PurchaseOrderDetailId = e.PurchaseOrderDetailId,
                ProductId = e.ProductId,
                ProductName = string.Empty,
                Quantity = e.Quantity,
                UnitPrice = e.UnitPrice,
                DiscountPerc = e.DiscountPerc,
                TaxPerc = e.TaxPerc,
                LineTotal = e.LineTotal,
            }
        );

        await col.UpdateOneAsync(
            x => x.Id == id,
            update,
            new UpdateOptions { IsUpsert = true },
            ct
        );
    }

    public async Task ProjectAsync(PurchaseOrderStatusChangedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PurchaseOrderReadModel>(OrdersCol);

        var id = $"purchase_order:{e.PurchaseOrderId}";

        await col.UpdateOneAsync(
            x => x.Id == id,
            Builders<PurchaseOrderReadModel>.Update.Set(x => x.Status, e.Status),
            new UpdateOptions { IsUpsert = true },
            ct
        );
    }
}
