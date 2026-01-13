using SharedKernel;

namespace Domain.AccountingPeriods;

public static class AccountingPeriodErrors
{
    public static Error NotFound(Guid periodId) =>
        Error.NotFound(
            "AccountingPeriods.NotFound",
            $"The accounting period with the Id = '{periodId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "AccountingPeriods.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error DuplicatePeriod = Error.Conflict(
        "AccountingPeriods.DuplicatePeriod",
        "The accounting period for the specified year and month already exists"
    );
}
