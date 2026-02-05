using Application.Abstractions.Messaging;

namespace Application.Features.Roles.Create;

public sealed record CreateRoleCommand(string Name, string? Description, bool IsActive = true)
    : ICommand<Guid>;
