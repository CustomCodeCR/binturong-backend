using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using Domain.Products;
using MongoDB.Driver;

namespace Infrastructure.Projections.Inventory;

internal sealed class ProductProjection
    : IProjector<ProductCreatedDomainEvent>,
        IProjector<ProductUpdatedDomainEvent>,
        IProjector<ProductDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public ProductProjection(IMongoDatabase db) => _db = db;

    public Task ProjectAsync(ProductCreatedDomainEvent e, CancellationToken ct) =>
        UpsertAsync(e, ct, isInsert: true);

    public Task ProjectAsync(ProductUpdatedDomainEvent e, CancellationToken ct) =>
        UpsertAsync(e, ct, isInsert: false);

    public async Task ProjectAsync(ProductDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ProductReadModel>(MongoCollections.Products);
        await col.DeleteOneAsync(x => x.Id == $"product:{e.ProductId}", ct);
    }

    private async Task UpsertAsync(dynamic e, CancellationToken ct, bool isInsert)
    {
        var col = _db.GetCollection<ProductReadModel>(MongoCollections.Products);

        var id = $"product:{(Guid)e.ProductId}";
        var filter = Builders<ProductReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ProductReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ProductId, (Guid)e.ProductId)
            .Set(x => x.SKU, (string)e.SKU)
            .Set(x => x.Barcode, (string?)e.Barcode)
            .Set(x => x.Name, (string)e.Name)
            .Set(x => x.Description, (string?)e.Description)
            .Set(x => x.CategoryId, (Guid?)e.CategoryId)
            .Set(x => x.UomId, (Guid?)e.UomId)
            .Set(x => x.TaxId, (Guid?)e.TaxId)
            .Set(x => x.BasePrice, (decimal)e.BasePrice)
            .Set(x => x.AverageCost, (decimal)e.AverageCost)
            .Set(x => x.IsService, (bool)e.IsService)
            .Set(x => x.IsActive, (bool)e.IsActive)
            .Set(x => x.UpdatedAt, (DateTime)e.UpdatedAt);

        if (isInsert)
        {
            update = update
                .SetOnInsert(x => x.CreatedAt, (DateTime)e.CreatedAt)
                .SetOnInsert(x => x.IsPublished, false)
                .SetOnInsert(x => x.ImageS3Keys, new List<string>());
        }

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}
