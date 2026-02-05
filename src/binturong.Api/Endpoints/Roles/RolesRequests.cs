namespace Api.Endpoints.Roles;

public sealed record CreateRoleRequest(string Name, string? Description, bool IsActive = true);

public sealed record UpdateRoleRequest(string Name, string? Description, bool IsActive);

public sealed record SetRoleScopesRequest(IReadOnlyList<Guid> ScopeIds);
