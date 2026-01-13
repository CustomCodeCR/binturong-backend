using SharedKernel;

namespace Domain.WarehouseStocks;

public sealed record WarehouseStockCreatedDomainEvent(Guid StockId) : IDomainEvent;
