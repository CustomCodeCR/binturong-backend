using SharedKernel;

namespace Domain.UnitsOfMeasure;

public static class UnitOfMeasureErrors
{
    public static Error NotFound(Guid uomId) =>
        Error.NotFound(
            "UnitsOfMeasure.NotFound",
            $"The unit of measure with the Id = '{uomId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "UnitsOfMeasure.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error CodeNotUnique = Error.Conflict(
        "UnitsOfMeasure.CodeNotUnique",
        "The provided unit of measure code is not unique"
    );

    public static readonly Error CodeIsRequired = Error.Validation(
        "UnitsOfMeasure.CodeIsRequired",
        "Unit of measure code is required"
    );

    public static readonly Error NameIsRequired = Error.Validation(
        "UnitsOfMeasure.NameIsRequired",
        "Unit of measure name is required"
    );

    public static readonly Error CannotDeleteInUse = Error.Conflict(
        "UnitsOfMeasure.InUse",
        "The unit of measure cannot be deleted because it is in use"
    );

    public static readonly Error Inactive = Error.Failure(
        "UnitsOfMeasure.Inactive",
        "The unit of measure is inactive"
    );
}
