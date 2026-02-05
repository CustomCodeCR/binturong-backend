using Application.Abstractions.Messaging;

namespace Application.Features.Inventory.Movements.RegisterServiceExit;

public sealed record RegisterServiceExitCommand(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    decimal UnitCost,
    string? Notes,
    int? SourceId
) : ICommand<Guid>;
