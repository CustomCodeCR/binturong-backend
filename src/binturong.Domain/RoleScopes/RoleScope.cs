using SharedKernel;

namespace Domain.RoleScopes;

public sealed class RoleScope : Entity
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public Guid ScopeId { get; set; }

    public Domain.Roles.Role? Role { get; set; }
    public Domain.Scopes.Scope? Scope { get; set; }
}
