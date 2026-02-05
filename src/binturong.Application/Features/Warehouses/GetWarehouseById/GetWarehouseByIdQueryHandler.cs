using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Warehouses.GetWarehouseById;

internal sealed class GetWarehouseByIdQueryHandler
    : IQueryHandler<GetWarehouseByIdQuery, WarehouseReadModel>
{
    private readonly IMongoDatabase _db;

    public GetWarehouseByIdQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<WarehouseReadModel>> Handle(
        GetWarehouseByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<WarehouseReadModel>(MongoCollections.Warehouses);
        var id = $"warehouse:{query.WarehouseId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

        if (doc is null)
            return Result.Failure<WarehouseReadModel>(
                Error.NotFound("Warehouses.NotFound", $"Warehouse '{query.WarehouseId}' not found")
            );

        return Result.Success(doc);
    }
}
