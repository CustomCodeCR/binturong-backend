using SharedKernel;

namespace Domain.RoleScopes;

public static class RoleScopeErrors
{
    public static readonly Error Duplicate = Error.Conflict(
        "RoleScopes.Duplicate",
        "The scope is already assigned to the role"
    );

    public static readonly Error Unauthorized = Error.Failure(
        "RoleScopes.Unauthorized",
        "You are not authorized to perform this action."
    );
}
