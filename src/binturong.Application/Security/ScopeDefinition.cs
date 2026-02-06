namespace Application.Security;

public sealed record ScopeDefinition(string Code, string Description, params string[] DefaultRoles);
