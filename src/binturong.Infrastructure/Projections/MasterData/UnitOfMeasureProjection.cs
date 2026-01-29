using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using Domain.UnitsOfMeasure;
using MongoDB.Driver;

namespace Infrastructure.Projections.MasterData;

internal sealed class UnitOfMeasureProjection
    : IProjector<UnitOfMeasureCreatedDomainEvent>,
        IProjector<UnitOfMeasureUpdatedDomainEvent>,
        IProjector<UnitOfMeasureDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public UnitOfMeasureProjection(IMongoDatabase db) => _db = db;

    public Task ProjectAsync(UnitOfMeasureCreatedDomainEvent e, CancellationToken ct) =>
        UpsertAsync(
            uomId: e.UomId,
            code: e.Code,
            name: e.Name,
            isActive: e.IsActive,
            createdAt: e.CreatedAt,
            updatedAt: e.UpdatedAt,
            ct
        );

    public Task ProjectAsync(UnitOfMeasureUpdatedDomainEvent e, CancellationToken ct) =>
        UpsertAsync(
            uomId: e.UomId,
            code: e.Code,
            name: e.Name,
            isActive: e.IsActive,
            createdAt: null, // no tocar createdAt
            updatedAt: e.UpdatedAt,
            ct
        );

    public async Task ProjectAsync(UnitOfMeasureDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<UnitOfMeasureReadModel>(MongoCollections.UnitsOfMeasure);

        var id = $"uom:{e.UomId}";
        var filter = Builders<UnitOfMeasureReadModel>.Filter.Eq(x => x.Id, id);

        await col.DeleteOneAsync(filter, ct);
    }

    private async Task UpsertAsync(
        Guid uomId,
        string code,
        string name,
        bool isActive,
        DateTime? createdAt,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<UnitOfMeasureReadModel>(MongoCollections.UnitsOfMeasure);

        var id = $"uom:{uomId}";
        var filter = Builders<UnitOfMeasureReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<UnitOfMeasureReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.UomId, uomId)
            .Set(x => x.Code, code)
            .Set(x => x.Name, name)
            .Set(x => x.IsActive, isActive)
            .Set(x => x.UpdatedAt, updatedAt);

        if (createdAt is not null)
        {
            update = update.SetOnInsert(x => x.CreatedAt, createdAt.Value);
        }

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}
