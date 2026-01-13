using SharedKernel;

namespace Domain.PurchaseOrders;

public sealed record PurchaseOrderCreatedDomainEvent(Guid PurchaseOrderId) : IDomainEvent;
