using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
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
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetProductCategoriesQueryHandler(
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

        await _bus.AuditAsync(
            _currentUser.UserId,
            "ProductCategories",
            "ProductCategory",
            null,
            "CATEGORY_LIST_READ",
            string.Empty,
            $"search={query.Search}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<ProductCategoryReadModel>>(docs);
    }
}
