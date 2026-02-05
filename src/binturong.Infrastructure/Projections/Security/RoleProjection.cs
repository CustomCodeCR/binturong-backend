using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using Domain.Roles;
using Domain.RoleScopes;
using Domain.Scopes;
using MongoDB.Driver;

namespace Infrastructure.Projections.Security;

internal sealed class RoleProjection
    : IProjector<RoleCreatedDomainEvent>,
        IProjector<RoleUpdatedDomainEvent>,
        IProjector<RoleDeletedDomainEvent>,
        IProjector<RoleScopeAssignedDomainEvent>,
        IProjector<RoleScopeRemovedDomainEvent>,
        IProjector<ScopeCreatedDomainEvent>,
        IProjector<ScopeUpdatedDomainEvent>,
        IProjector<ScopeDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public RoleProjection(IMongoDatabase db) => _db = db;

    public Task ProjectAsync(RoleCreatedDomainEvent e, CancellationToken ct) =>
        UpsertRoleAsync(e.RoleId, e.Name, e.Description, e.IsActive, ct);

    public Task ProjectAsync(RoleUpdatedDomainEvent e, CancellationToken ct) =>
        UpsertRoleAsync(e.RoleId, e.Name, e.Description, e.IsActive, ct);

    public async Task ProjectAsync(RoleDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);
        await col.DeleteOneAsync(x => x.Id == $"role:{e.RoleId}", ct);
    }

    public async Task ProjectAsync(RoleScopeAssignedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);
        var id = $"role:{e.RoleId}";

        var pull = Builders<RoleReadModel>.Update.PullFilter(
            x => x.Scopes,
            s => s.Code == e.ScopeCode
        );
        await col.UpdateOneAsync(x => x.Id == id, pull, cancellationToken: ct);

        var push = Builders<RoleReadModel>.Update.Push(
            x => x.Scopes,
            new ScopeReadModel
            {
                ScopeId = Guid.NewGuid(),
                Code = e.ScopeCode,
                Description = null,
            }
        );

        await col.UpdateOneAsync(x => x.Id == id, push, cancellationToken: ct);
    }

    public async Task ProjectAsync(RoleScopeRemovedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);
        var id = $"role:{e.RoleId}";

        var update = Builders<RoleReadModel>.Update.PullFilter(
            x => x.Scopes,
            s => s.Code == e.ScopeCode
        );

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }

    public Task ProjectAsync(ScopeCreatedDomainEvent e, CancellationToken ct) =>
        UpsertScopeAsync(e.ScopeId, e.Code, e.Description, ct);

    public Task ProjectAsync(ScopeUpdatedDomainEvent e, CancellationToken ct) =>
        UpsertScopeAsync(e.ScopeId, e.Code, e.Description, ct);

    public async Task ProjectAsync(ScopeDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ScopeCatalogReadModel>(MongoCollections.Scopes);
        await col.DeleteOneAsync(x => x.Id == $"scope:{e.ScopeId}", ct);

        var roles = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);
        var update = Builders<RoleReadModel>.Update.PullFilter(
            x => x.Scopes,
            s => s.Code == e.ScopeId.ToString()
        );

        await roles.UpdateManyAsync(
            Builders<RoleReadModel>.Filter.Empty,
            update,
            cancellationToken: ct
        );
    }

    private async Task UpsertRoleAsync(
        Guid roleId,
        string name,
        string? description,
        bool isActive,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);

        var id = $"role:{roleId}";
        var filter = Builders<RoleReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<RoleReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.RoleId, Guid.NewGuid())
            .Set(x => x.Name, name)
            .Set(x => x.Description, description)
            .Set(x => x.IsActive, isActive)
            .SetOnInsert(x => x.Scopes, new List<ScopeReadModel>());

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task UpsertScopeAsync(
        Guid scopeId,
        string code,
        string? description,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ScopeCatalogReadModel>(MongoCollections.Scopes);

        var id = $"scope:{scopeId}";
        var filter = Builders<ScopeCatalogReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ScopeCatalogReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ScopeId, 0)
            .Set(x => x.Code, code)
            .Set(x => x.Description, description);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}

public sealed class ScopeCatalogReadModel
{
    public string Id { get; init; } = default!;
    public int ScopeId { get; init; }
    public string Code { get; init; } = default!;
    public string? Description { get; init; }
}
