using SharedKernel;

namespace Domain.UserRoles;

public sealed class UserRole : Entity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public Domain.Users.User? User { get; set; }
    public Domain.Roles.Role? Role { get; set; }

    public void RaiseAssigned() => Raise(new UserRoleAssignedDomainEvent(UserId, RoleId));

    public void RaiseRemoved() => Raise(new UserRoleRemovedDomainEvent(UserId, RoleId));
}
