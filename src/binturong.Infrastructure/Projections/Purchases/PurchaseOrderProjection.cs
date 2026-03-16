using Application.Abstractions.Projections;
using Application.ReadModels.CRM;
using Application.ReadModels.Inventory;
using Application.ReadModels.MasterData;
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
    private const string SuppliersCol = "suppliers";
    private const string BranchesCol = "branches";
    private const string ProductsCol = "products";

    private readonly IMongoDatabase _db;

    public PurchaseOrderProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(PurchaseOrderCreatedDomainEvent e, CancellationToken ct)
    {
        var orders = _db.GetCollection<PurchaseOrderReadModel>(OrdersCol);

        var supplierName = await GetSupplierNameAsync(e.SupplierId, ct);
        var branchName = await GetBranchNameAsync(e.BranchId, ct);

        var id = $"purchase_order:{e.PurchaseOrderId}";
        var filter = Builders<PurchaseOrderReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<PurchaseOrderReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .Set(x => x.PurchaseOrderId, e.PurchaseOrderId)
            .Set(x => x.Code, e.Code)
            .Set(x => x.SupplierId, e.SupplierId)
            .Set(x => x.SupplierName, supplierName)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.BranchName, branchName)
            .Set(x => x.RequestId, e.RequestId)
            .Set(x => x.OrderDate, e.OrderDate)
            .Set(x => x.Status, e.Status)
            .Set(x => x.Currency, e.Currency)
            .Set(x => x.ExchangeRate, e.ExchangeRate)
            .Set(x => x.Subtotal, e.Subtotal)
            .Set(x => x.Taxes, e.Taxes)
            .Set(x => x.Discounts, e.Discounts)
            .Set(x => x.Total, e.Total)
            .SetOnInsert(x => x.Lines, new List<PurchaseOrderLineReadModel>());

        await orders.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(PurchaseOrderLineAddedDomainEvent e, CancellationToken ct)
    {
        var orders = _db.GetCollection<PurchaseOrderReadModel>(OrdersCol);

        var id = $"purchase_order:{e.PurchaseOrderId}";
        var filter = Builders<PurchaseOrderReadModel>.Filter.Eq(x => x.Id, id);

        var productName = await GetProductNameAsync(e.ProductId, ct);

        var line = new PurchaseOrderLineReadModel
        {
            PurchaseOrderDetailId = e.PurchaseOrderDetailId,
            ProductId = e.ProductId,
            ProductName = productName,
            Quantity = e.Quantity,
            UnitPrice = e.UnitPrice,
            DiscountPerc = e.DiscountPerc,
            TaxPerc = e.TaxPerc,
            LineTotal = e.LineTotal,
        };

        await orders.UpdateOneAsync(
            filter,
            Builders<PurchaseOrderReadModel>
                .Update.SetOnInsert(x => x.Id, id)
                .SetOnInsert(x => x.PurchaseOrderId, e.PurchaseOrderId),
            new UpdateOptions { IsUpsert = true },
            ct
        );

        await orders.UpdateOneAsync(
            filter,
            Builders<PurchaseOrderReadModel>.Update.Push(x => x.Lines, line),
            new UpdateOptions { IsUpsert = false },
            ct
        );
    }

    public async Task ProjectAsync(PurchaseOrderStatusChangedDomainEvent e, CancellationToken ct)
    {
        var orders = _db.GetCollection<PurchaseOrderReadModel>(OrdersCol);

        var id = $"purchase_order:{e.PurchaseOrderId}";

        await orders.UpdateOneAsync(
            x => x.Id == id,
            Builders<PurchaseOrderReadModel>.Update.Set(x => x.Status, e.Status),
            new UpdateOptions { IsUpsert = false },
            ct
        );
    }

    private async Task<string> GetSupplierNameAsync(Guid supplierId, CancellationToken ct)
    {
        var suppliers = _db.GetCollection<SupplierReadModel>(SuppliersCol);
        var supplierDocId = $"supplier:{supplierId}";

        var supplier = await suppliers.Find(x => x.Id == supplierDocId).FirstOrDefaultAsync(ct);

        return supplier?.TradeName ?? string.Empty;
    }

    private async Task<string?> GetBranchNameAsync(Guid? branchId, CancellationToken ct)
    {
        if (branchId is null)
        {
            return null;
        }

        var branches = _db.GetCollection<BranchReadModel>(BranchesCol);
        var branchDocId = $"branch:{branchId.Value}";

        var branch = await branches.Find(x => x.Id == branchDocId).FirstOrDefaultAsync(ct);

        return branch?.Name;
    }

    private async Task<string> GetProductNameAsync(Guid productId, CancellationToken ct)
    {
        var products = _db.GetCollection<ProductReadModel>(ProductsCol);
        var productDocId = $"product:{productId}";

        var product = await products.Find(x => x.Id == productDocId).FirstOrDefaultAsync(ct);

        return product?.Name ?? string.Empty;
    }
}
