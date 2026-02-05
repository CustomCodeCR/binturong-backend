using Application.Abstractions.Messaging;

namespace Application.Features.Warehouses.Update;

public sealed record UpdateWarehouseCommand(
    Guid WarehouseId,
    string Code,
    string Name,
    string? Description,
    bool IsActive
) : ICommand;
