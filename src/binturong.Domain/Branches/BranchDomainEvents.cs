using SharedKernel;

namespace Domain.Branches;

public sealed record BranchCreatedDomainEvent(
    Guid BranchId,
    string Code,
    string Name,
    string Address,
    string Phone,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record BranchUpdatedDomainEvent(
    Guid BranchId,
    string Code,
    string Name,
    string Address,
    string Phone,
    bool IsActive,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record BranchDeletedDomainEvent(Guid BranchId) : IDomainEvent;

public sealed record BranchActivatedDomainEvent(Guid BranchId, DateTime UpdatedAt) : IDomainEvent;

public sealed record BranchDeactivatedDomainEvent(Guid BranchId, DateTime UpdatedAt) : IDomainEvent;
