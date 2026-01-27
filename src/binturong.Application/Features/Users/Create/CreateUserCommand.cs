using Application.Abstractions.Messaging;

namespace Application.Features.Users.Create;

public sealed record CreateUserCommand(
    string Username,
    string Email,
    string Password,
    bool IsActive = true
) : ICommand<Guid>;
