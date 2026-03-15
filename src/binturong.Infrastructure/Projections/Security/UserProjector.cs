using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using Domain.Users;
using MongoDB.Driver;

namespace Infrastructure.Projections.Security;

internal sealed class UserProjection
    : IProjector<UserRegisteredDomainEvent>,
        IProjector<UserUpdatedDomainEvent>,
        IProjector<UserDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public UserProjection(IMongoDatabase db)
    {
        _db = db;
    }

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
            .SetOnInsert(x => x.LastLogin, null)
            .SetOnInsert(x => x.MustChangePassword, false)
            .SetOnInsert(x => x.FailedAttempts, 0)
            .SetOnInsert(x => x.LockedUntil, null)
            .SetOnInsert(x => x.Roles, new List<UserRoleReadModel>())
            .SetOnInsert(x => x.Scopes, new List<string>());

        await col.UpdateOneAsync(
            x => x.UserId == e.UserId,
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
}
