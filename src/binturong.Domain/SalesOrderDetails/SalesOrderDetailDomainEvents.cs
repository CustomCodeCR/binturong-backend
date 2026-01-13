using SharedKernel;

namespace Domain.SalesOrderDetails;

public sealed record SalesOrderDetailCreatedDomainEvent(Guid SalesOrderDetailId) : IDomainEvent;
