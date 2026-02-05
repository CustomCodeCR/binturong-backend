using SharedKernel;

namespace Domain.Roles;

public sealed record RoleCreatedDomainEvent(
    Guid RoleId,
    string Name,
    string? Description,
    bool IsActive
) : IDomainEvent;

public sealed record RoleUpdatedDomainEvent(
    Guid RoleId,
    string Name,
    string? Description,
    bool IsActive
) : IDomainEvent;

public sealed record RoleDeletedDomainEvent(Guid RoleId) : IDomainEvent;
