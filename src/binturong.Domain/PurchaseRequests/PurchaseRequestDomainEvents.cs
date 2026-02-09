using SharedKernel;

namespace Domain.PurchaseRequests;

public sealed record PurchaseRequestCreatedDomainEvent(
    Guid RequestId,
    string Code,
    Guid? BranchId,
    Guid? RequestedById,
    DateTime RequestDateUtc,
    string Status,
    string? Notes
) : IDomainEvent;
