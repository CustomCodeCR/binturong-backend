using SharedKernel;

namespace Domain.RoleScopes;

public sealed record RoleScopeAssignedDomainEvent(Guid RoleId, Guid ScopeId, string ScopeCode)
    : IDomainEvent;

public sealed record RoleScopeRemovedDomainEvent(Guid RoleId, Guid ScopeId, string ScopeCode)
    : IDomainEvent;
