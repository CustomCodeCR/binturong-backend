using Application.Abstractions.Messaging;

namespace Application.Features.Inventory.Movements.RegisterPurchaseEntry;

public sealed record RegisterPurchaseEntryCommand(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    decimal UnitCost,
    string? Notes,
    int? SourceId
) : ICommand<Guid>;
