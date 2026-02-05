using SharedKernel;

namespace Domain.WarehouseStocks;

public sealed record WarehouseStockChangedDomainEvent(
    Guid WarehouseStockId,
    Guid ProductId,
    Guid WarehouseId,
    decimal CurrentStock,
    decimal MinStock,
    decimal MaxStock,
    DateTime UpdatedAt
) : IDomainEvent;
