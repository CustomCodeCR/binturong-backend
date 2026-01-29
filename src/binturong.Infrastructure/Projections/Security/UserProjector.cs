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

    public UserProjection(IMongoDatabase db) => _db = db;

    public Task ProjectAsync(UserRegisteredDomainEvent e, CancellationToken ct) =>
        UpsertAsync(
            userId: e.UserId,
            username: e.Username,
            email: e.Email,
            isActive: e.IsActive,
            lastLogin: null,
            mustChangePassword: false,
            failedAttempts: 0,
            lockedUntil: null,
            createdAt: e.CreatedAt,
            updatedAt: e.UpdatedAt,
            ct
        );

    public Task ProjectAsync(UserUpdatedDomainEvent e, CancellationToken ct) =>
        UpsertAsync(
            userId: e.UserId,
            username: e.Username,
            email: e.Email,
            isActive: e.IsActive,
            lastLogin: e.LastLogin,
            mustChangePassword: e.MustChangePassword,
            failedAttempts: e.FailedAttempts,
            lockedUntil: e.LockedUntil,
            createdAt: null, // no tocar createdAt en updates
            updatedAt: e.UpdatedAt,
            ct
        );

    public async Task ProjectAsync(UserDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        var id = $"user:{e.UserId}";
        var filter = Builders<UserReadModel>.Filter.Eq(x => x.Id, id);

        await col.DeleteOneAsync(filter, ct);
    }

    private async Task UpsertAsync(
        Guid userId,
        string username,
        string email,
        bool isActive,
        DateTime? lastLogin,
        bool mustChangePassword,
        int failedAttempts,
        DateTime? lockedUntil,
        DateTime? createdAt,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        var id = $"user:{userId}";
        var filter = Builders<UserReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<UserReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.UserId, userId)
            .Set(x => x.Username, username)
            .Set(x => x.Email, email)
            .Set(x => x.IsActive, isActive)
            .Set(x => x.LastLogin, lastLogin)
            .Set(x => x.MustChangePassword, mustChangePassword)
            .Set(x => x.FailedAttempts, failedAttempts)
            .Set(x => x.LockedUntil, lockedUntil)
            .Set(x => x.UpdatedAt, updatedAt);

        if (createdAt is not null)
        {
            update = update.SetOnInsert(x => x.CreatedAt, createdAt.Value);
        }

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}
