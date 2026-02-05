using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Branches.GetBranches;

internal sealed class GetBranchesQueryHandler
    : IQueryHandler<GetBranchesQuery, IReadOnlyList<BranchReadModel>>
{
    private readonly IMongoDatabase _db;

    public GetBranchesQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<BranchReadModel>>> Handle(
        GetBranchesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);

        var filter = BuildFilter(query.Search);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.UpdatedAt)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<BranchReadModel>>(docs);
    }

    private static FilterDefinition<BranchReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<BranchReadModel>.Filter.Empty;

        var s = search.Trim();

        // OR over relevant fields
        return Builders<BranchReadModel>.Filter.Or(
            Builders<BranchReadModel>.Filter.Regex(
                x => x.Code,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<BranchReadModel>.Filter.Regex(
                x => x.Name,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<BranchReadModel>.Filter.Regex(
                x => x.Address,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<BranchReadModel>.Filter.Regex(
                x => x.Phone,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            )
        );
    }
}
