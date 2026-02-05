using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Products.GetProductById;

internal sealed class GetProductByIdQueryHandler
    : IQueryHandler<GetProductByIdQuery, ProductReadModel>
{
    private readonly IMongoDatabase _db;

    public GetProductByIdQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<ProductReadModel>> Handle(
        GetProductByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ProductReadModel>(MongoCollections.Products);
        var id = $"product:{query.ProductId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc is null)
            return Result.Failure<ProductReadModel>(
                Error.NotFound("Products.NotFound", $"Product '{query.ProductId}' not found")
            );

        return Result.Success(doc);
    }
}
