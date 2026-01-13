using SharedKernel;

namespace Domain.SalesOrders;

public sealed record SalesOrderCreatedDomainEvent(Guid SalesOrderId) : IDomainEvent;
