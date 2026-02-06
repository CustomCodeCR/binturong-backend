using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
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
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetProductsQueryHandler(
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

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Products",
            "Product",
            null,
            "PRODUCT_LIST_READ",
            string.Empty,
            $"search={query.Search}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<ProductReadModel>>(docs);
    }
}
