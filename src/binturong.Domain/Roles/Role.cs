using SharedKernel;

namespace Domain.Roles;

public sealed class Role : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public ICollection<Domain.UserRoles.UserRole> UserRoles { get; set; } =
        new List<Domain.UserRoles.UserRole>();
    public ICollection<Domain.RoleScopes.RoleScope> RoleScopes { get; set; } =
        new List<Domain.RoleScopes.RoleScope>();
}
