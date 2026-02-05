using SharedKernel;

namespace Domain.Roles;

public sealed class Role : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Domain.UserRoles.UserRole> UserRoles { get; set; } =
        new List<Domain.UserRoles.UserRole>();

    public ICollection<Domain.RoleScopes.RoleScope> RoleScopes { get; set; } =
        new List<Domain.RoleScopes.RoleScope>();

    public void RaiseCreated() =>
        Raise(new RoleCreatedDomainEvent(Id, Name, Description, IsActive));

    public void RaiseUpdated() =>
        Raise(new RoleUpdatedDomainEvent(Id, Name, Description, IsActive));

    public void RaiseDeleted() => Raise(new RoleDeletedDomainEvent(Id));
}
