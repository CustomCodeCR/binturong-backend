using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Warehouses.GetWarehouses;

internal sealed class GetWarehousesQueryHandler
    : IQueryHandler<GetWarehousesQuery, IReadOnlyList<WarehouseReadModel>>
{
    private const string Module = "Warehouses";
    private const string Entity = "Warehouse";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetWarehousesQueryHandler(
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

    public async Task<Result<IReadOnlyList<WarehouseReadModel>>> Handle(
        GetWarehousesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<WarehouseReadModel>(MongoCollections.Warehouses);

        var filter = BuildFilter(query.Search, query.BranchId);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.UpdatedAt)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null, // list query
                "WAREHOUSES_READ",
                string.Empty,
                $"search={query.Search ?? ""}; branchId={(query.BranchId is null ? "" : query.BranchId.Value.ToString())}; skip={query.Skip}; take={query.Take}; returned={docs.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<WarehouseReadModel>>(docs);
    }

    private static FilterDefinition<WarehouseReadModel> BuildFilter(string? search, Guid? branchId)
    {
        var builder = Builders<WarehouseReadModel>.Filter;

        var filters = new List<FilterDefinition<WarehouseReadModel>>();

        if (branchId is not null)
        {
            filters.Add(builder.Eq(x => x.BranchId, branchId.Value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();

            filters.Add(
                builder.Or(
                    builder.Regex(x => x.Code, new MongoDB.Bson.BsonRegularExpression(s, "i")),
                    builder.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(s, "i")),
                    builder.Regex(
                        x => x.BranchCode,
                        new MongoDB.Bson.BsonRegularExpression(s, "i")
                    ),
                    builder.Regex(x => x.BranchName, new MongoDB.Bson.BsonRegularExpression(s, "i"))
                )
            );
        }

        if (filters.Count == 0)
            return builder.Empty;

        return builder.And(filters);
    }
}
