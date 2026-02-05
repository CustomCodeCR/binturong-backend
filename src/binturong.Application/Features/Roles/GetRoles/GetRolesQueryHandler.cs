using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Roles.GetRoles;

internal sealed class GetRolesQueryHandler
    : IQueryHandler<GetRolesQuery, IReadOnlyList<RoleReadModel>>
{
    private readonly IMongoDatabase _db;

    public GetRolesQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<RoleReadModel>>> Handle(
        GetRolesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);

        var f = Builders<RoleReadModel>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            f = Builders<RoleReadModel>.Filter.Or(
                Builders<RoleReadModel>.Filter.Regex(
                    x => x.Name,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<RoleReadModel>.Filter.Regex(
                    x => x.Description,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                )
            );
        }

        var docs = await col.Find(f).Skip(query.Skip).Limit(query.Take).ToListAsync(ct);
        return Result.Success((IReadOnlyList<RoleReadModel>)docs);
    }
}
