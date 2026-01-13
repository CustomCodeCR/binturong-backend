using SharedKernel;

namespace Domain.RoleScopes;

public static class RoleScopeErrors
{
    public static Error NotFound(Guid roleScopeId) =>
        Error.NotFound(
            "RoleScopes.NotFound",
            $"The role scope with the Id = '{roleScopeId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("RoleScopes.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error DuplicateAssignment = Error.Conflict(
        "RoleScopes.DuplicateAssignment",
        "The role already has this scope assigned"
    );
}
