using Application.Abstractions.Messaging;

namespace Application.Features.Roles.Delete;

public sealed record DeleteRoleCommand(Guid RoleId) : ICommand;
