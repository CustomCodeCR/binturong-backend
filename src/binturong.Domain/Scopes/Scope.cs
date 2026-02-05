using SharedKernel;

namespace Domain.Scopes;

public sealed class Scope : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Domain.RoleScopes.RoleScope> RoleScopes { get; set; } =
        new List<Domain.RoleScopes.RoleScope>();

    public ICollection<Domain.UserScopes.UserScope> UserScopes { get; set; } =
        new List<Domain.UserScopes.UserScope>();

    public void RaiseCreated() => Raise(new ScopeCreatedDomainEvent(Id, Code, Description));

    public void RaiseUpdated() => Raise(new ScopeUpdatedDomainEvent(Id, Code, Description));

    public void RaiseDeleted() => Raise(new ScopeDeletedDomainEvent(Id));
}
