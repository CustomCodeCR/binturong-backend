namespace Application.ReadModels.Security;

public sealed class UserReadModel
{
    public string Id { get; init; } = default!; // "user:{UserId}"
    public int UserId { get; init; }

    public string Username { get; init; } = default!;
    public string Email { get; init; } = default!;

    public bool IsActive { get; init; }
    public DateTime? LastLogin { get; init; }

    public bool MustChangePassword { get; init; }
    public int FailedAttempts { get; init; }
    public DateTime? LockedUntil { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IReadOnlyList<UserRoleReadModel> Roles { get; init; } = [];
    public IReadOnlyList<string> Scopes { get; init; } = [];
}

public sealed class UserRoleReadModel
{
    public int RoleId { get; init; }
    public string Name { get; init; } = default!;
}
