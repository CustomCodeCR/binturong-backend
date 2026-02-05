using Application.Abstractions.Messaging;

namespace Application.Features.Inventory.Movements.RegisterPhysicalCountAdjustment;

public sealed record RegisterPhysicalCountAdjustmentCommand(
    Guid ProductId,
    Guid WarehouseId,
    decimal CountedStock,
    decimal UnitCost,
    string Justification
) : ICommand<Guid>;
