using SharedKernel;

namespace Domain.ServiceOrders;

public sealed record ServiceOrderCreatedDomainEvent(Guid ServiceOrderId) : IDomainEvent;
