namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string UsersRead = "users.read";
    public const string UsersCreate = "users.create";
    public const string UsersUpdate = "users.update";
    public const string UsersDelete = "users.delete";

    public const string UsersAssignRole = "users.roles.assign";
    public const string UsersAssignScopes = "users.assign_scopes";
}
