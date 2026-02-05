using Application.Abstractions.Messaging;

namespace Application.Features.Roles.SetRoleScopes;

public sealed record SetRoleScopesCommand(
    Guid RoleId,
    IReadOnlyList<Guid> ScopeIds,
    Guid ActorUserId
) : ICommand;
