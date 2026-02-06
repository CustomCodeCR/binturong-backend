namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string RolesRead = "roles.read";
    public const string RolesCreate = "roles.create";
    public const string RolesUpdate = "roles.update";
    public const string RolesDelete = "roles.delete";

    public const string RolesAssignScopes = "roles.scopes.assign";
    public const string RolesSystemProtect = "roles.system.protect";
}
