using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using Domain.UserRoles;
using Domain.Users;
using MongoDB.Driver;

namespace Infrastructure.Projections.Security;

internal sealed class UserProjection
    : IProjector<UserRegisteredDomainEvent>,
        IProjector<UserUpdatedDomainEvent>,
        IProjector<UserDeletedDomainEvent>,
        IProjector<UserRoleAssignedDomainEvent>,
        IProjector<UserRoleRemovedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public UserProjection(IMongoDatabase db) => _db = db;

    // =========================
    // USER ROOT
    // =========================

    public async Task ProjectAsync(UserRegisteredDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        var id = $"user:{e.UserId}";

        var update = Builders<UserReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.UserId, e.UserId)
            .Set(x => x.Username, e.Username)
            .Set(x => x.Email, e.Email)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.CreatedAt, e.CreatedAt)
            .Set(x => x.UpdatedAt, e.UpdatedAt)
            .SetOnInsert(x => x.Roles, new List<UserRoleReadModel>())
            .SetOnInsert(x => x.Scopes, new List<string>());

        await col.UpdateOneAsync(
            x => x.Id == id,
            update,
            new UpdateOptions { IsUpsert = true },
            ct
        );
    }

    public async Task ProjectAsync(UserUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        var update = Builders<UserReadModel>
            .Update.Set(x => x.Username, e.Username)
            .Set(x => x.Email, e.Email)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.LastLogin, e.LastLogin)
            .Set(x => x.MustChangePassword, e.MustChangePassword)
            .Set(x => x.FailedAttempts, e.FailedAttempts)
            .Set(x => x.LockedUntil, e.LockedUntil)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        await col.UpdateOneAsync(x => x.UserId == e.UserId, update, cancellationToken: ct);
    }

    public async Task ProjectAsync(UserDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);
        await col.DeleteOneAsync(x => x.UserId == e.UserId, ct);
    }

    // =========================
    // ROLES
    // =========================

    public async Task ProjectAsync(UserRoleAssignedDomainEvent e, CancellationToken ct)
    {
        await RebuildUserSecurityAsync(e.UserId, ct);
    }

    public async Task ProjectAsync(UserRoleRemovedDomainEvent e, CancellationToken ct)
    {
        await RebuildUserSecurityAsync(e.UserId, ct);
    }

    // =========================
    // REBUILD SNAPSHOT
    // =========================

    private async Task RebuildUserSecurityAsync(Guid userId, CancellationToken ct)
    {
        var users = _db.GetCollection<UserReadModel>(MongoCollections.Users);
        var roles = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);

        // 1️⃣ Obtener roles del usuario (desde Mongo Roles)
        var roleDocs = await roles
            .Find(r => r.Scopes.Any()) // roles válidos
            .ToListAsync(ct);

        var userRoles = roleDocs
            .Select(r => new UserRoleReadModel { RoleId = r.RoleId, Name = r.Name })
            .ToList();

        // 2️⃣ Recalcular scopes
        var scopes = roleDocs
            .SelectMany(r => r.Scopes)
            .Select(s => s.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // 3️⃣ Update atomico
        await users.UpdateOneAsync(
            x => x.UserId == userId,
            Builders<UserReadModel>
                .Update.Set(x => x.Roles, userRoles)
                .Set(x => x.Scopes, scopes)
                .Set(x => x.UpdatedAt, DateTime.UtcNow),
            cancellationToken: ct
        );
    }
}
