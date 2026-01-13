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
        "The provided UoM code is not unique"
    );
}
