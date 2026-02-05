using SharedKernel;

namespace Domain.UserRoles;

public static class UserRoleErrors
{
    public static readonly Error Duplicate = Error.Conflict(
        "UserRoles.Duplicate",
        "The role is already assigned to the user"
    );

    public static readonly Error Unauthorized = Error.Failure(
        "UserRoles.Unauthorized",
        "You are not authorized to perform this action."
    );
}
