using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.ProductCategories.GetProductCategoryById;

internal sealed class GetProductCategoryByIdQueryHandler
    : IQueryHandler<GetProductCategoryByIdQuery, ProductCategoryReadModel>
{
    private readonly IMongoDatabase _db;

    public GetProductCategoryByIdQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<ProductCategoryReadModel>> Handle(
        GetProductCategoryByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ProductCategoryReadModel>(MongoCollections.ProductCategories);
        var id = $"category:{query.CategoryId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc is null)
            return Result.Failure<ProductCategoryReadModel>(
                Error.NotFound(
                    "ProductCategories.NotFound",
                    $"Category '{query.CategoryId}' not found"
                )
            );

        return Result.Success(doc);
    }
}
