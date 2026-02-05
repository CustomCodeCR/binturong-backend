using Application.Abstractions.Messaging;

namespace Application.Features.Warehouses.Create;

public sealed record CreateWarehouseCommand(
    Guid BranchId,
    string Code,
    string Name,
    string? Description,
    bool IsActive = true
) : ICommand<Guid>;
