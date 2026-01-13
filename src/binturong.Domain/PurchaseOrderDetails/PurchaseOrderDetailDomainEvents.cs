using SharedKernel;

namespace Domain.PurchaseOrderDetails;

public sealed record PurchaseOrderDetailCreatedDomainEvent(Guid PurchaseOrderDetailId)
    : IDomainEvent;
