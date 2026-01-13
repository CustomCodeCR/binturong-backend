using SharedKernel;

namespace Domain.UserRoles;

public sealed record UserRoleAssignedDomainEvent(Guid UserRoleId) : IDomainEvent;
