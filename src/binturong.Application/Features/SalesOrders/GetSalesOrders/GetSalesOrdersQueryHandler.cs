using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.SalesOrders.GetSalesOrders;

internal sealed class GetSalesOrdersQueryHandler
    : IQueryHandler<GetSalesOrdersQuery, IReadOnlyList<SalesOrderReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetSalesOrdersQueryHandler(
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

    public async Task<Result<IReadOnlyList<SalesOrderReadModel>>> Handle(
        GetSalesOrdersQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var filter = BuildFilter(query.Search);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.OrderDate)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Sales",
            "SalesOrder",
            null,
            "SALES_ORDER_LIST_READ",
            string.Empty,
            $"search={query.Search}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<SalesOrderReadModel>>(docs);
    }

    private static FilterDefinition<SalesOrderReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<SalesOrderReadModel>.Filter.Empty;

        var s = search.Trim();

        return Builders<SalesOrderReadModel>.Filter.Or(
            Builders<SalesOrderReadModel>.Filter.Regex(
                x => x.Code,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<SalesOrderReadModel>.Filter.Regex(
                x => x.ClientName,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<SalesOrderReadModel>.Filter.Regex(
                x => x.Status,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<SalesOrderReadModel>.Filter.Regex(
                x => x.Currency,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            )
        );
    }
}
