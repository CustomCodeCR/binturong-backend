namespace Application.ReadModels.Security;

public sealed class RoleReadModel
{
    public string Id { get; init; } = default!; // "role:{RoleId}"
    public int RoleId { get; init; }

    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public bool IsActive { get; init; }

    public IReadOnlyList<ScopeReadModel> Scopes { get; init; } = [];
}

public sealed class ScopeReadModel
{
    public int ScopeId { get; init; }
    public string Code { get; init; } = default!;
    public string? Description { get; init; }
}
