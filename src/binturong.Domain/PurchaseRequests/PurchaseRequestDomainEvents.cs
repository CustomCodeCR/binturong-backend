using SharedKernel;

namespace Domain.PurchaseRequests;

public sealed record PurchaseRequestCreatedDomainEvent(Guid RequestId) : IDomainEvent;
