using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.ProductCategories.GetProductCategoryById;

internal sealed class GetProductCategoryByIdQueryHandler
    : IQueryHandler<GetProductCategoryByIdQuery, ProductCategoryReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetProductCategoryByIdQueryHandler(
        IMongoDatabase db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

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

        await _bus.AuditAsync(
            _currentUser.UserId,
            "ProductCategories",
            "ProductCategory",
            query.CategoryId,
            "CATEGORY_READ",
            string.Empty,
            $"categoryId={query.CategoryId}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(doc);
    }
}
