using SharedKernel;

namespace Domain.CostCenters;

public static class CostCenterErrors
{
    public static Error NotFound(Guid costCenterId) =>
        Error.NotFound(
            "CostCenters.NotFound",
            $"The cost center with the Id = '{costCenterId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("CostCenters.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error CodeNotUnique = Error.Conflict(
        "CostCenters.CodeNotUnique",
        "The provided cost center code is not unique"
    );
}
