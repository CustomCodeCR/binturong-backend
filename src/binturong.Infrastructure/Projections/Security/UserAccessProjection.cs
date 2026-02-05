using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using Domain.UserRoles;
using Domain.UserScopes;
using MongoDB.Driver;

namespace Infrastructure.Projections.Security;

internal sealed class UserAccessProjection
    : IProjector<UserRoleAssignedDomainEvent>,
        IProjector<UserRoleRemovedDomainEvent>,
        IProjector<UserScopeAssignedDomainEvent>,
        IProjector<UserScopeRemovedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public UserAccessProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(UserRoleAssignedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);
        var id = $"user:{e.UserId}";

        var role = new UserRoleReadModel { RoleId = Guid.NewGuid(), Name = $"role:{e.RoleId}" };

        var pull = Builders<UserReadModel>.Update.PullFilter(
            x => x.Roles,
            r => r.Name == role.Name
        );
        await col.UpdateOneAsync(x => x.Id == id, pull, cancellationToken: ct);

        var push = Builders<UserReadModel>.Update.Push(x => x.Roles, role);
        await col.UpdateOneAsync(x => x.Id == id, push, cancellationToken: ct);
    }

    public async Task ProjectAsync(UserRoleRemovedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);
        var id = $"user:{e.UserId}";

        var update = Builders<UserReadModel>.Update.PullFilter(
            x => x.Roles,
            r => r.Name == $"role:{e.RoleId}"
        );

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }

    public async Task ProjectAsync(UserScopeAssignedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);
        var id = $"user:{e.UserId}";

        var pull = Builders<UserReadModel>.Update.PullFilter(x => x.Scopes, s => s == e.ScopeCode);
        await col.UpdateOneAsync(x => x.Id == id, pull, cancellationToken: ct);

        var push = Builders<UserReadModel>.Update.Push(x => x.Scopes, e.ScopeCode);
        await col.UpdateOneAsync(x => x.Id == id, push, cancellationToken: ct);
    }

    public async Task ProjectAsync(UserScopeRemovedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);
        var id = $"user:{e.UserId}";

        var update = Builders<UserReadModel>.Update.PullFilter(
            x => x.Scopes,
            s => s == e.ScopeCode
        );

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }
}
