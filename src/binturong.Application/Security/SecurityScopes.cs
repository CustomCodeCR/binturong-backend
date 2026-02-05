namespace Application.Security;

public static class SecurityScopes
{
    public const string RolesRead = "roles.read";
    public const string RolesCreate = "roles.create";
    public const string RolesUpdate = "roles.update";
    public const string RolesDelete = "roles.delete";
    public const string RolesAssignScopes = "roles.assign_scopes";

    public const string UsersAssignRole = "users.assign_role";
    public const string UsersAssignScopes = "users.assign_scopes";

    public const string ScopesRead = "scopes.read";
    public const string ScopesCreate = "scopes.create";
    public const string ScopesUpdate = "scopes.update";
    public const string ScopesDelete = "scopes.delete";
}
