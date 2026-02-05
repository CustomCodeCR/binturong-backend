using Domain.InventoryMovementTypes;
using SharedKernel;

namespace Domain.InventoryMovements;

public sealed record InventoryMovementRegisteredDomainEvent(
    Guid MovementId,
    Guid ProductId,
    Guid? WarehouseFrom,
    Guid? WarehouseTo,
    InventoryMovementType MovementType,
    DateTime MovementDate,
    decimal Quantity,
    decimal UnitCost,
    string SourceModule,
    int? SourceId,
    string? Notes
) : IDomainEvent;
