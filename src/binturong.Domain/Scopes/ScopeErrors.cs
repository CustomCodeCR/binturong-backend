using SharedKernel;

namespace Domain.Scopes;

public static class ScopeErrors
{
    public static Error NotFound(Guid scopeId) =>
        Error.NotFound("Scopes.NotFound", $"The scope with the Id = '{scopeId}' was not found");

    public static Error Unauthorized() =>
        Error.Failure("Scopes.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error CodeNotUnique = Error.Conflict(
        "Scopes.CodeNotUnique",
        "The provided scope code is not unique"
    );
}
