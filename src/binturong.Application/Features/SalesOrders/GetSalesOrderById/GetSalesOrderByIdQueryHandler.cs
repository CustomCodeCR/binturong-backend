using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.SalesOrders.GetSalesOrderById;

internal sealed class GetSalesOrderByIdQueryHandler
    : IQueryHandler<GetSalesOrderByIdQuery, SalesOrderReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetSalesOrderByIdQueryHandler(
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

    public async Task<Result<SalesOrderReadModel>> Handle(
        GetSalesOrderByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var doc = await col.Find(x => x.SalesOrderId == query.SalesOrderId).FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Sales",
            "SalesOrder",
            query.SalesOrderId,
            "SALES_ORDER_READ",
            string.Empty,
            $"salesOrderId={query.SalesOrderId}; found={(doc is not null)}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<SalesOrderReadModel>(
                Error.NotFound(
                    "SalesOrders.NotFound",
                    $"Sales order '{query.SalesOrderId}' not found"
                )
            )
            : Result.Success(doc);
    }
}
