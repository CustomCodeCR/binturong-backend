using Application.Abstractions.Messaging;

namespace Application.Features.Users.SetUserScopes;

public sealed record SetUserScopesCommand(Guid UserId, IReadOnlyList<Guid> ScopeIds) : ICommand;
