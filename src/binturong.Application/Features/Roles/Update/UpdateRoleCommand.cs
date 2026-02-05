using Application.Abstractions.Messaging;

namespace Application.Features.Roles.Update;

public sealed record UpdateRoleCommand(Guid RoleId, string Name, string? Description, bool IsActive)
    : ICommand;
