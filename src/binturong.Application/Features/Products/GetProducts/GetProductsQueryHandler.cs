using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Products.GetProducts;

internal sealed class GetProductsQueryHandler
    : IQueryHandler<GetProductsQuery, IReadOnlyList<ProductReadModel>>
{
    private readonly IMongoDatabase _db;

    public GetProductsQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<ProductReadModel>>> Handle(
        GetProductsQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ProductReadModel>(MongoCollections.Products);

        var filter = string.IsNullOrWhiteSpace(query.Search)
            ? Builders<ProductReadModel>.Filter.Empty
            : Builders<ProductReadModel>.Filter.Or(
                Builders<ProductReadModel>.Filter.Regex(
                    x => x.Name,
                    new BsonRegularExpression(query.Search.Trim(), "i")
                ),
                Builders<ProductReadModel>.Filter.Regex(
                    x => x.SKU,
                    new BsonRegularExpression(query.Search.Trim(), "i")
                ),
                Builders<ProductReadModel>.Filter.Regex(
                    x => x.Barcode,
                    new BsonRegularExpression(query.Search.Trim(), "i")
                )
            );

        var docs = await col.Find(filter)
            .SortBy(x => x.Name)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<ProductReadModel>>(docs);
    }
}
