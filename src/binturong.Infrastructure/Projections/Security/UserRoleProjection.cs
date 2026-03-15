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

    public UserRoleProjection(IMongoDatabase db)
    {
        _db = db;
    }

    public async Task ProjectAsync(UserRoleAssignedDomainEvent e, CancellationToken ct)
    {
        var users = _db.GetCollection<UserReadModel>(MongoCollections.Users);
        var roles = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);

        var role = await roles.Find(x => x.RoleId == e.RoleId).FirstOrDefaultAsync(ct);
        if (role is null)
        {
            return;
        }

        var userRole = new UserRoleReadModel { RoleId = role.RoleId, Name = role.Name };

        var scopes = role
            .Scopes.Select(x => x.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var update = Builders<UserReadModel>
            .Update.Set(x => x.Roles, new List<UserRoleReadModel> { userRole })
            .Set(x => x.Scopes, scopes)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await users.UpdateOneAsync(x => x.UserId == e.UserId, update, cancellationToken: ct);
    }

    public async Task ProjectAsync(UserRoleRemovedDomainEvent e, CancellationToken ct)
    {
        var users = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        var update = Builders<UserReadModel>
            .Update.Set(x => x.Roles, new List<UserRoleReadModel>())
            .Set(x => x.Scopes, new List<string>())
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await users.UpdateOneAsync(x => x.UserId == e.UserId, update, cancellationToken: ct);
    }
}
