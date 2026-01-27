using SharedKernel;

namespace Domain.Users;

public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Username,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record UserUpdatedDomainEvent(
    Guid UserId,
    string Username,
    string Email,
    bool IsActive,
    DateTime? LastLogin,
    bool MustChangePassword,
    int FailedAttempts,
    DateTime? LockedUntil,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record UserDeletedDomainEvent(Guid UserId) : IDomainEvent;
