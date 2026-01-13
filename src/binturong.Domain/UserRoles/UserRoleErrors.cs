using SharedKernel;

namespace Domain.UserRoles;

public static class UserRoleErrors
{
    public static Error NotFound(Guid userRoleId) =>
        Error.NotFound(
            "UserRoles.NotFound",
            $"The user role with the Id = '{userRoleId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("UserRoles.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error DuplicateAssignment = Error.Conflict(
        "UserRoles.DuplicateAssignment",
        "The user already has this role assigned"
    );
}
