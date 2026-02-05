using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using Domain.Scopes;
using MongoDB.Driver;

namespace Infrastructure.Projections.Security;

internal sealed class ScopeProjection : IProjector<ScopeCreatedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public ScopeProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(ScopeCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ScopeReadModel>(MongoCollections.Scopes);

        var id = $"scope:{e.ScopeId}";

        await col.UpdateOneAsync(
            x => x.Id == id,
            Builders<ScopeReadModel>
                .Update.SetOnInsert(x => x.Id, id)
                .SetOnInsert(x => x.ScopeId, e.ScopeId)
                .Set(x => x.Code, e.Code)
                .Set(x => x.Description, e.Description),
            new UpdateOptions { IsUpsert = true },
            ct
        );
    }
}
