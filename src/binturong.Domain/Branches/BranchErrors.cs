using SharedKernel;

namespace Domain.Branches;

public static class BranchErrors
{
    public static Error NotFound(Guid branchId) =>
        Error.NotFound("Branches.NotFound", $"The branch with the Id = '{branchId}' was not found");

    public static Error Unauthorized() =>
        Error.Failure("Branches.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error CodeNotUnique = Error.Conflict(
        "Branches.CodeNotUnique",
        "The provided branch code is not unique"
    );
}
