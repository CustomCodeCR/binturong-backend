using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using Domain.UserRoles;
using MongoDB.Driver;

namespace Infrastructure.Projections.Security;

internal sealed class UserRoleProjection
    : IProjector<UserRoleAssignedDomainEvent>,
        IProjector<UserRoleRemovedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public UserRoleProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(UserRoleAssignedDomainEvent e, CancellationToken ct)
    {
        var users = _db.GetCollection<UserReadModel>(MongoCollections.Users);
        var roles = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);

        var role = await roles.Find(r => r.RoleId == e.RoleId).FirstOrDefaultAsync(ct);
        if (role is null)
            return;

        var roleItem = new UserRoleReadModel { RoleId = e.RoleId, Name = role.Name };

        await users.UpdateOneAsync(
            x => x.UserId == e.UserId,
            Builders<UserReadModel>.Update.AddToSet(x => x.Roles, roleItem),
            cancellationToken: ct
        );
    }

    public async Task ProjectAsync(UserRoleRemovedDomainEvent e, CancellationToken ct)
    {
        var users = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        await users.UpdateOneAsync(
            x => x.UserId == e.UserId,
            Builders<UserReadModel>.Update.PullFilter(x => x.Roles, r => r.RoleId == e.RoleId),
            cancellationToken: ct
        );
    }
}
