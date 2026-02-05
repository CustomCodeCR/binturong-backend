using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.ProductCategories.GetProductCategories;

internal sealed class GetProductCategoriesQueryHandler
    : IQueryHandler<GetProductCategoriesQuery, IReadOnlyList<ProductCategoryReadModel>>
{
    private readonly IMongoDatabase _db;

    public GetProductCategoriesQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<ProductCategoryReadModel>>> Handle(
        GetProductCategoriesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ProductCategoryReadModel>(MongoCollections.ProductCategories);

        var filter = string.IsNullOrWhiteSpace(query.Search)
            ? Builders<ProductCategoryReadModel>.Filter.Empty
            : Builders<ProductCategoryReadModel>.Filter.Regex(
                x => x.Name,
                new BsonRegularExpression(query.Search.Trim(), "i")
            );

        var docs = await col.Find(filter)
            .SortBy(x => x.Name)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<ProductCategoryReadModel>>(docs);
    }
}
