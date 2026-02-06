using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Warehouses.GetWarehouseById;

internal sealed class GetWarehouseByIdQueryHandler
    : IQueryHandler<GetWarehouseByIdQuery, WarehouseReadModel>
{
    private const string Module = "Warehouses";
    private const string Entity = "Warehouse";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetWarehouseByIdQueryHandler(
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

    public async Task<Result<WarehouseReadModel>> Handle(
        GetWarehouseByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<WarehouseReadModel>(MongoCollections.Warehouses);
        var id = $"warehouse:{query.WarehouseId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

        if (doc is null)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    _currentUser.UserId,
                    Module,
                    Entity,
                    query.WarehouseId,
                    "WAREHOUSE_READ_NOT_FOUND",
                    string.Empty,
                    $"warehouseId={query.WarehouseId}",
                    _request.IpAddress,
                    _request.UserAgent
                ),
                ct
            );

            return Result.Failure<WarehouseReadModel>(
                Error.NotFound("Warehouses.NotFound", $"Warehouse '{query.WarehouseId}' not found")
            );
        }

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                query.WarehouseId,
                "WAREHOUSE_READ",
                string.Empty,
                $"warehouseId={query.WarehouseId}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success(doc);
    }
}
