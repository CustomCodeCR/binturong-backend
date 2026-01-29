using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Users.GetUsers;

internal sealed class GetUsersQueryHandler
    : IQueryHandler<GetUsersQuery, IReadOnlyList<UserReadModel>>
{
    private readonly IMongoDatabase _db;

    public GetUsersQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<UserReadModel>>> Handle(
        GetUsersQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        var filter = Builders<UserReadModel>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Ajustá campos según tu UserReadModel real
            var s = query.Search.Trim();
            filter = Builders<UserReadModel>.Filter.Or(
                Builders<UserReadModel>.Filter.Regex(
                    x => x.Username,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<UserReadModel>.Filter.Regex(
                    x => x.Email,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                )
            );
        }

        var docs = await col.Find(filter)
            .SortByDescending(x => x.UpdatedAt)
            .Skip(query.Skip)
            .Limit(query.PageSize)
            .ToListAsync(ct);

        return Result.Success((IReadOnlyList<UserReadModel>)docs);
    }
}
