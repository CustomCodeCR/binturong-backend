using SharedKernel;

namespace Domain.UserScopes;

public static class UserScopeErrors
{
    public static readonly Error Duplicate = Error.Conflict(
        "UserScopes.Duplicate",
        "The scope is already assigned to the user"
    );

    public static readonly Error Unauthorized = Error.Failure(
        "UserScopes.Unauthorized",
        "You are not authorized to perform this action."
    );
}
