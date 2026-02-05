using Application.Abstractions.Messaging;

namespace Application.Features.Users.RemoveRole;

public sealed record RemoveRoleFromUserCommand(Guid UserId, Guid RoleId) : ICommand;
