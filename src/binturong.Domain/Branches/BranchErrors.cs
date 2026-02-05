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

    public static readonly Error CodeIsRequired = Error.Validation(
        "Branches.CodeIsRequired",
        "Branch code is required"
    );

    public static readonly Error NameIsRequired = Error.Validation(
        "Branches.NameIsRequired",
        "Branch name is required"
    );

    public static readonly Error AddressIsRequired = Error.Validation(
        "Branches.AddressIsRequired",
        "Branch address is required"
    );

    public static readonly Error PhoneIsRequired = Error.Validation(
        "Branches.PhoneIsRequired",
        "Branch phone is required"
    );

    public static readonly Error AlreadyActive = Error.Conflict(
        "Branches.AlreadyActive",
        "The branch is already active"
    );

    public static readonly Error AlreadyInactive = Error.Conflict(
        "Branches.AlreadyInactive",
        "The branch is already inactive"
    );
}
