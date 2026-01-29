namespace Api.Endpoints.Users;

public sealed record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    bool IsActive = true
);

public sealed record UpdateUserRequest(
    string Username,
    string Email,
    bool IsActive,
    DateTime? LastLogin,
    bool MustChangePassword,
    int FailedAttempts,
    DateTime? LockedUntil
);
