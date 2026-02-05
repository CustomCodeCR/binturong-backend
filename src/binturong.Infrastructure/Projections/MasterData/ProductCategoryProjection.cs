using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using Domain.ProductCategories;
using MongoDB.Driver;

namespace Infrastructure.Projections.MasterData;

internal sealed class ProductCategoryProjection
    : IProjector<ProductCategoryCreatedDomainEvent>,
        IProjector<ProductCategoryUpdatedDomainEvent>,
        IProjector<ProductCategoryDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public ProductCategoryProjection(IMongoDatabase db) => _db = db;

    public Task ProjectAsync(ProductCategoryCreatedDomainEvent e, CancellationToken ct) =>
        UpsertAsync(e.CategoryId, e.Name, e.Description, e.IsActive, ct);

    public Task ProjectAsync(ProductCategoryUpdatedDomainEvent e, CancellationToken ct) =>
        UpsertAsync(e.CategoryId, e.Name, e.Description, e.IsActive, ct);

    public async Task ProjectAsync(ProductCategoryDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ProductCategoryReadModel>(MongoCollections.ProductCategories);
        await col.DeleteOneAsync(x => x.Id == $"category:{e.CategoryId}", ct);
    }

    private async Task UpsertAsync(
        Guid categoryId,
        string name,
        string? description,
        bool isActive,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ProductCategoryReadModel>(MongoCollections.ProductCategories);

        var id = $"category:{categoryId}";
        var filter = Builders<ProductCategoryReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ProductCategoryReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.CategoryId, categoryId)
            .Set(x => x.Name, name)
            .Set(x => x.Description, description)
            .Set(x => x.IsActive, isActive);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}
