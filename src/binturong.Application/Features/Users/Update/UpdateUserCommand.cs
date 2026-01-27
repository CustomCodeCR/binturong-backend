using Application.Abstractions.Messaging;

namespace Application.Features.Users.Update;

public sealed record UpdateUserCommand(
    Guid UserId,
    string Username,
    string Email,
    bool IsActive,
    DateTime? LastLogin,
    bool MustChangePassword,
    int FailedAttempts,
    DateTime? LockedUntil
) : ICommand;
