using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Taxes.GetTaxes;

internal sealed class GetTaxesQueryHandler
    : IQueryHandler<GetTaxesQuery, IReadOnlyList<TaxReadModel>>
{
    private readonly IMongoDatabase _db;

    public GetTaxesQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<TaxReadModel>>> Handle(
        GetTaxesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<TaxReadModel>(MongoCollections.Taxes);

        var filter = BuildFilter(query.Search);

        var items = await col.Find(filter)
            .SortBy(x => x.Code)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<TaxReadModel>>(items);
    }

    private static FilterDefinition<TaxReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<TaxReadModel>.Filter.Empty;

        var s = search.Trim();

        // Case-insensitive contains on Code/Name
        var name = Builders<TaxReadModel>.Filter.Regex(
            x => x.Name,
            new MongoDB.Bson.BsonRegularExpression(s, "i")
        );

        var code = Builders<TaxReadModel>.Filter.Regex(
            x => x.Code,
            new MongoDB.Bson.BsonRegularExpression(s, "i")
        );

        return Builders<TaxReadModel>.Filter.Or(name, code);
    }
}
