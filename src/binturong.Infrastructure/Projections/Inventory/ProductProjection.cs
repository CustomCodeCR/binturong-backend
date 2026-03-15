using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using Application.ReadModels.MasterData;
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
        UpsertCreatedAsync(e, ct);

    public Task ProjectAsync(ProductUpdatedDomainEvent e, CancellationToken ct) =>
        UpsertUpdatedAsync(e, ct);

    public async Task ProjectAsync(ProductDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ProductReadModel>(MongoCollections.Products);
        await col.DeleteOneAsync(x => x.Id == $"product:{e.ProductId}", ct);
    }

    private async Task UpsertCreatedAsync(ProductCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ProductReadModel>(MongoCollections.Products);

        var categories = _db.GetCollection<ProductCategoryReadModel>(
            MongoCollections.ProductCategories
        );
        var uoms = _db.GetCollection<UnitOfMeasureReadModel>(MongoCollections.UnitsOfMeasure);
        var taxes = _db.GetCollection<TaxReadModel>(MongoCollections.Taxes);

        var category = e.CategoryId.HasValue
            ? await categories
                .Find(x => x.Id == $"category:{e.CategoryId.Value}")
                .FirstOrDefaultAsync(ct)
            : null;

        var uom = e.UomId.HasValue
            ? await uoms.Find(x => x.Id == $"uom:{e.UomId.Value}").FirstOrDefaultAsync(ct)
            : null;

        var tax = e.TaxId.HasValue
            ? await taxes.Find(x => x.Id == $"tax:{e.TaxId.Value}").FirstOrDefaultAsync(ct)
            : null;

        var id = $"product:{e.ProductId}";
        var filter = Builders<ProductReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ProductReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ProductId, e.ProductId)
            .Set(x => x.SKU, e.SKU)
            .Set(x => x.Barcode, string.IsNullOrWhiteSpace(e.Barcode) ? null : e.Barcode)
            .Set(x => x.Name, e.Name)
            .Set(
                x => x.Description,
                string.IsNullOrWhiteSpace(e.Description) ? null : e.Description
            )
            .Set(x => x.CategoryId, e.CategoryId)
            .Set(x => x.CategoryName, category?.Name)
            .Set(x => x.UomId, e.UomId)
            .Set(x => x.UomCode, uom?.Code)
            .Set(x => x.UomName, uom?.Name)
            .Set(x => x.TaxId, e.TaxId)
            .Set(x => x.TaxCode, tax?.Code)
            .Set(x => x.TaxPercentage, tax?.Percentage ?? 0m)
            .Set(x => x.BasePrice, e.BasePrice)
            .Set(x => x.AverageCost, e.AverageCost)
            .Set(x => x.IsService, e.IsService)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.UpdatedAt, e.UpdatedAt)
            .SetOnInsert(x => x.CreatedAt, e.CreatedAt)
            .SetOnInsert(x => x.IsPublished, false)
            .SetOnInsert(x => x.ImageS3Keys, new List<string>());

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task UpsertUpdatedAsync(ProductUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ProductReadModel>(MongoCollections.Products);

        var categories = _db.GetCollection<ProductCategoryReadModel>(
            MongoCollections.ProductCategories
        );
        var uoms = _db.GetCollection<UnitOfMeasureReadModel>(MongoCollections.UnitsOfMeasure);
        var taxes = _db.GetCollection<TaxReadModel>(MongoCollections.Taxes);

        var category = e.CategoryId.HasValue
            ? await categories
                .Find(x => x.Id == $"category:{e.CategoryId.Value}")
                .FirstOrDefaultAsync(ct)
            : null;

        var uom = e.UomId.HasValue
            ? await uoms.Find(x => x.Id == $"uom:{e.UomId.Value}").FirstOrDefaultAsync(ct)
            : null;

        var tax = e.TaxId.HasValue
            ? await taxes.Find(x => x.Id == $"tax:{e.TaxId.Value}").FirstOrDefaultAsync(ct)
            : null;

        var id = $"product:{e.ProductId}";
        var filter = Builders<ProductReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ProductReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ProductId, e.ProductId)
            .Set(x => x.SKU, e.SKU)
            .Set(x => x.Barcode, string.IsNullOrWhiteSpace(e.Barcode) ? null : e.Barcode)
            .Set(x => x.Name, e.Name)
            .Set(
                x => x.Description,
                string.IsNullOrWhiteSpace(e.Description) ? null : e.Description
            )
            .Set(x => x.CategoryId, e.CategoryId)
            .Set(x => x.CategoryName, category?.Name)
            .Set(x => x.UomId, e.UomId)
            .Set(x => x.UomCode, uom?.Code)
            .Set(x => x.UomName, uom?.Name)
            .Set(x => x.TaxId, e.TaxId)
            .Set(x => x.TaxCode, tax?.Code)
            .Set(x => x.TaxPercentage, tax?.Percentage ?? 0m)
            .Set(x => x.BasePrice, e.BasePrice)
            .Set(x => x.AverageCost, e.AverageCost)
            .Set(x => x.IsService, e.IsService)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}
