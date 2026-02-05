using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using Domain.InventoryMovements;
using Domain.InventoryMovementTypes;
using MongoDB.Driver;

namespace Infrastructure.Projections.Inventory;

internal sealed class InventoryMovementProjection
    : IProjector<InventoryMovementRegisteredDomainEvent>
{
    private readonly IMongoDatabase _db;

    public InventoryMovementProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(InventoryMovementRegisteredDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InventoryMovementReadModel>(
            MongoCollections.InventoryMovements
        );

        var id = $"inv_mov:{e.MovementId}";
        var filter = Builders<InventoryMovementReadModel>.Filter.Eq(x => x.Id, id);

        var doc = new InventoryMovementReadModel
        {
            Id = id,
            MovementId = e.MovementId,
            ProductId = e.ProductId,
            ProductName = "",

            MovementType = e.MovementType,
            MovementTypeCode = e.MovementType.Code(),
            MovementTypeDescription = e.MovementType.Description(),
            Sign = e.MovementType.Sign(),

            WarehouseFromId = e.WarehouseFrom,
            WarehouseFromName = null,
            WarehouseToId = e.WarehouseTo,
            WarehouseToName = null,

            MovementDate = e.MovementDate,
            Quantity = e.Quantity,
            UnitCost = e.UnitCost,
            SourceModule = e.SourceModule,
            SourceId = e.SourceId,
            Notes = e.Notes,
        };

        var update = Builders<InventoryMovementReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .Set(x => x, doc);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}
