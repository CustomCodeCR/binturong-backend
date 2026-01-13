using SharedKernel;

namespace Domain.RoleScopes;

public sealed record RoleScopeAssignedDomainEvent(Guid RoleScopeId) : IDomainEvent;
