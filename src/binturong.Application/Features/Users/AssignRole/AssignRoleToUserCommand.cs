using Application.Abstractions.Messaging;

namespace Application.Features.Users.AssignRole;

public sealed record AssignRoleToUserCommand(Guid UserId, Guid RoleId, bool ReplaceExisting = true)
    : ICommand;
