using SharedKernel;

namespace Domain.Scopes;

public sealed class Scope : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Domain.RoleScopes.RoleScope> RoleScopes { get; set; } =
        new List<Domain.RoleScopes.RoleScope>();
}
