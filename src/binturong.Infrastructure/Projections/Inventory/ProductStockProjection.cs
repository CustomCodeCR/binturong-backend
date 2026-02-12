using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using Application.ReadModels.MasterData;
using Domain.WarehouseStocks;
using MongoDB.Driver;

namespace Infrastructure.Projections.Inventory;

internal sealed class ProductStockProjection : IProjector<WarehouseStockChangedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public ProductStockProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(WarehouseStockChangedDomainEvent e, CancellationToken ct)
    {
        var stocks = _db.GetCollection<ProductStockReadModel>(MongoCollections.ProductStocks);
        var warehouses = _db.GetCollection<WarehouseReadModel>(MongoCollections.Warehouses);

        var whDoc = await warehouses
            .Find(x => x.Id == $"warehouse:{e.WarehouseId}")
            .FirstOrDefaultAsync(ct);

        var stockId = $"stock:{e.ProductId}";
        var filter = Builders<ProductStockReadModel>.Filter.Eq(x => x.Id, stockId);

        var upsert = Builders<ProductStockReadModel>
            .Update.SetOnInsert(x => x.Id, stockId)
            .SetOnInsert(x => x.ProductId, e.ProductId)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        await stocks.UpdateOneAsync(filter, upsert, new UpdateOptions { IsUpsert = true }, ct);

        var pull = Builders<ProductStockReadModel>.Update.PullFilter(
            x => x.Warehouses,
            w => w.WarehouseId == e.WarehouseId
        );
        await stocks.UpdateOneAsync(filter, pull, cancellationToken: ct);

        var entry = new WarehouseStockReadModel
        {
            WarehouseId = e.WarehouseId,
            WarehouseCode = whDoc?.Code ?? "",
            WarehouseName = whDoc?.Name ?? "",
            BranchId = whDoc?.BranchId ?? Guid.Empty,
            CurrentStock = e.CurrentStock,
            MinStock = e.MinStock,
            MaxStock = e.MaxStock,
        };

        var push = Builders<ProductStockReadModel>
            .Update.Push(x => x.Warehouses, entry)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        await stocks.UpdateOneAsync(filter, push, cancellationToken: ct);

        var doc = await stocks.Find(x => x.Id == stockId).FirstOrDefaultAsync(ct);
        if (doc is not null)
        {
            var total = doc.Warehouses.Sum(w => w.CurrentStock);
            await stocks.UpdateOneAsync(
                filter,
                Builders<ProductStockReadModel>.Update.Set(x => x.TotalStock, total),
                cancellationToken: ct
            );
        }
    }
}
