using SharedKernel;

namespace Domain.UserRoles;

public sealed record UserRoleAssignedDomainEvent(Guid UserId, Guid RoleId) : IDomainEvent;

public sealed record UserRoleRemovedDomainEvent(Guid UserId, Guid RoleId) : IDomainEvent;
