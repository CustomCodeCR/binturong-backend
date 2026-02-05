using MongoDB.Bson.Serialization.Attributes;

namespace Application.ReadModels.Security;

public sealed class UserReadModel
{
    [BsonId]
    public string Id { get; init; } = default!; // "user:{UserId}"

    public Guid UserId { get; init; }
    public string Username { get; init; } = default!;
    public string Email { get; init; } = default!;

    public bool IsActive { get; init; }
    public DateTime? LastLogin { get; init; }

    public bool MustChangePassword { get; init; }
    public int FailedAttempts { get; init; }
    public DateTime? LockedUntil { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public List<UserRoleReadModel> Roles { get; init; } = new();
    public List<string> Scopes { get; init; } = new();
}

public sealed class RoleReadModel
{
    [BsonId]
    public string Id { get; init; } = default!; // "role:{RoleId}"

    public Guid RoleId { get; init; }
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public bool IsActive { get; init; }

    public List<ScopeReadModel> Scopes { get; init; } = new();
}

public sealed class ScopeReadModel
{
    [BsonId]
    public string Id { get; init; } = default!; // "scope:{ScopeId}"

    public Guid ScopeId { get; init; }
    public string Code { get; init; } = default!;
    public string? Description { get; init; }
}

public sealed class UserRoleReadModel
{
    public Guid RoleId { get; init; }
    public string Name { get; init; } = default!;
}
