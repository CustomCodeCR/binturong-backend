using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using Domain.Taxes;
using MongoDB.Driver;

namespace Infrastructure.Projections.MasterData;

internal sealed class TaxProjection
    : IProjector<TaxCreatedDomainEvent>,
        IProjector<TaxUpdatedDomainEvent>,
        IProjector<TaxDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public TaxProjection(IMongoDatabase db) => _db = db;

    public Task ProjectAsync(TaxCreatedDomainEvent e, CancellationToken ct) =>
        UpsertAsync(
            taxId: e.TaxId,
            name: e.Name,
            code: e.Code,
            percentage: e.Percentage,
            isActive: e.IsActive,
            createdAt: e.CreatedAt,
            updatedAt: e.UpdatedAt,
            ct
        );

    public Task ProjectAsync(TaxUpdatedDomainEvent e, CancellationToken ct) =>
        UpsertAsync(
            taxId: e.TaxId,
            name: e.Name,
            code: e.Code,
            percentage: e.Percentage,
            isActive: e.IsActive,
            createdAt: null, // no tocar createdAt
            updatedAt: e.UpdatedAt,
            ct
        );

    public async Task ProjectAsync(TaxDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<TaxReadModel>(MongoCollections.Taxes);

        var id = $"tax:{e.TaxId}";
        var filter = Builders<TaxReadModel>.Filter.Eq(x => x.Id, id);

        await col.DeleteOneAsync(filter, ct);
    }

    private async Task UpsertAsync(
        Guid taxId,
        string name,
        string code,
        decimal percentage,
        bool isActive,
        DateTime? createdAt,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<TaxReadModel>(MongoCollections.Taxes);

        var id = $"tax:{taxId}";
        var filter = Builders<TaxReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<TaxReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.TaxId, taxId)
            .Set(x => x.Name, name)
            .Set(x => x.Code, code)
            .Set(x => x.Percentage, percentage)
            .Set(x => x.IsActive, isActive)
            .Set(x => x.UpdatedAt, updatedAt);

        if (createdAt is not null)
        {
            update = update.SetOnInsert(x => x.CreatedAt, createdAt.Value);
        }

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}
