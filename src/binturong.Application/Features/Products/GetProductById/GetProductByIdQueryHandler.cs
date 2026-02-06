using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Products.GetProductById;

internal sealed class GetProductByIdQueryHandler
    : IQueryHandler<GetProductByIdQuery, ProductReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetProductByIdQueryHandler(
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

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Products",
            "Product",
            query.ProductId,
            "PRODUCT_READ",
            string.Empty,
            $"productId={query.ProductId}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(doc);
    }
}
