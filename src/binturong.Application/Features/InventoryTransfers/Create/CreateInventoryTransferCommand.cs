using Application.Abstractions.Messaging;

namespace Application.Features.InventoryTransfers.Create;

public sealed record CreateInventoryTransferLineDto(
    Guid ProductId,
    decimal Quantity,
    Guid FromWarehouseId,
    Guid ToWarehouseId
);

public sealed record CreateInventoryTransferCommand(
    Guid FromBranchId,
    Guid ToBranchId,
    string? Notes,
    Guid CreatedByUserId,
    IReadOnlyList<CreateInventoryTransferLineDto> Lines
) : ICommand<Guid>;
