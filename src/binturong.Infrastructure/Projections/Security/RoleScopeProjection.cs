using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using Domain.RoleScopes;
using MongoDB.Driver;

namespace Infrastructure.Projections.Security;

internal sealed class RoleScopeProjection
    : IProjector<RoleScopeAssignedDomainEvent>,
        IProjector<RoleScopeRemovedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public RoleScopeProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(RoleScopeAssignedDomainEvent e, CancellationToken ct)
    {
        var users = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        await users.UpdateManyAsync(
            x => x.Roles.Any(r => r.RoleId == e.RoleId),
            Builders<UserReadModel>.Update.AddToSet(x => x.Scopes, e.ScopeCode),
            cancellationToken: ct
        );
    }

    public async Task ProjectAsync(RoleScopeRemovedDomainEvent e, CancellationToken ct)
    {
        var users = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        await users.UpdateManyAsync(
            x => x.Roles.Any(r => r.RoleId == e.RoleId),
            Builders<UserReadModel>.Update.Pull(x => x.Scopes, e.ScopeCode),
            cancellationToken: ct
        );
    }
}
