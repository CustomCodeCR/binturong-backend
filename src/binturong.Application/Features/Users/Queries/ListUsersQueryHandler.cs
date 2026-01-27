using Application.Abstractions.Messaging;
using Application.ReadModels.Security;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Users.Queries;

internal sealed class ListUsersQueryHandler
    : IQueryHandler<ListUsersQuery, IReadOnlyList<UserReadModel>>
{
    private readonly IMongoDatabase _db;

    public ListUsersQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<UserReadModel>>> Handle(
        ListUsersQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        var docs = await col.Find(Builders<UserReadModel>.Filter.Empty)
            .SortByDescending(x => x.UpdatedAt)
            .Limit(query.Take)
            .ToListAsync(ct);

        return Result.Success((IReadOnlyList<UserReadModel>)docs);
    }
}
