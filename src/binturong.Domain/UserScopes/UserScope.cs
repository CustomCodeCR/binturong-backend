using SharedKernel;

namespace Domain.UserScopes;

public sealed class UserScope : Entity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ScopeId { get; set; }

    public Domain.Users.User? User { get; set; }
    public Domain.Scopes.Scope? Scope { get; set; }
}
