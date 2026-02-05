namespace Api.Endpoints.Users;

public sealed record AssignUserRoleRequest(Guid RoleId, bool ReplaceExisting = true);

public sealed record RemoveUserRoleRequest(Guid RoleId);

public sealed record SetUserScopesRequest(IReadOnlyList<Guid> ScopeIds);
