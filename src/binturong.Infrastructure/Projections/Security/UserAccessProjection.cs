using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using Domain.UserScopes;
using MongoDB.Driver;

namespace Infrastructure.Projections.Security;

internal sealed class UserAccessProjection
    : IProjector<UserScopeAssignedDomainEvent>,
        IProjector<UserScopeRemovedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public UserAccessProjection(IMongoDatabase db)
    {
        _db = db;
    }

    public async Task ProjectAsync(UserScopeAssignedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        await col.UpdateOneAsync(
            x => x.UserId == e.UserId,
            Builders<UserReadModel>.Update.AddToSet(x => x.Scopes, e.ScopeCode),
            cancellationToken: ct
        );
    }

    public async Task ProjectAsync(UserScopeRemovedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        await col.UpdateOneAsync(
            x => x.UserId == e.UserId,
            Builders<UserReadModel>.Update.Pull(x => x.Scopes, e.ScopeCode),
            cancellationToken: ct
        );
    }
}
