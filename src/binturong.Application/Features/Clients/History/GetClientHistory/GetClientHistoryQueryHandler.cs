using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Clients.History.GetClientHistory;

internal sealed class GetClientHistoryQueryHandler
    : IQueryHandler<GetClientHistoryQuery, IReadOnlyList<SalesOrderReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetClientHistoryQueryHandler(
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
        GetClientHistoryQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.ClientId, query.ClientId);

        if (query.From is not null)
            filter &= Builders<SalesOrderReadModel>.Filter.Gte(x => x.OrderDate, query.From.Value);

        if (query.To is not null)
            filter &= Builders<SalesOrderReadModel>.Filter.Lte(x => x.OrderDate, query.To.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            filter &= Builders<SalesOrderReadModel>.Filter.Eq(x => x.Status, query.Status.Trim());

        var docs = await col.Find(filter)
            .SortByDescending(x => x.OrderDate)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Clients",
            "ClientHistory",
            query.ClientId,
            "CLIENT_HISTORY_READ",
            string.Empty,
            $"clientId={query.ClientId}; from={query.From}; to={query.To}; status={query.Status}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<SalesOrderReadModel>>(docs);
    }
}
