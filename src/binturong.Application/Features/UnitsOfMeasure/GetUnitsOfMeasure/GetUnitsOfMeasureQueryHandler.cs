using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.UnitsOfMeasure.GetUnitsOfMeasure;

internal sealed class GetUnitsOfMeasureQueryHandler
    : IQueryHandler<GetUnitsOfMeasureQuery, IReadOnlyList<UnitOfMeasureReadModel>>
{
    private readonly IMongoDatabase _db;

    public GetUnitsOfMeasureQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<UnitOfMeasureReadModel>>> Handle(
        GetUnitsOfMeasureQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<UnitOfMeasureReadModel>(MongoCollections.UnitsOfMeasure);

        var filter = BuildFilter(query.Search);

        var docs = await col.Find(filter)
            .SortBy(x => x.Code)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<UnitOfMeasureReadModel>>(docs);
    }

    private static FilterDefinition<UnitOfMeasureReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<UnitOfMeasureReadModel>.Filter.Empty;

        var s = search.Trim();

        var code = Builders<UnitOfMeasureReadModel>.Filter.Regex(
            x => x.Code,
            new BsonRegularExpression(s, "i")
        );

        var name = Builders<UnitOfMeasureReadModel>.Filter.Regex(
            x => x.Name,
            new BsonRegularExpression(s, "i")
        );

        return Builders<UnitOfMeasureReadModel>.Filter.Or(code, name);
    }
}
