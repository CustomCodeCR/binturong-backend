using SharedKernel;

namespace Domain.Branches;

public sealed record BranchCreatedDomainEvent(Guid BranchId) : IDomainEvent;
