using SharedKernel;

namespace Domain.Payrolls;

public static class PayrollErrors
{
    public static Error NotFound(Guid payrollId) =>
        Error.NotFound(
            "Payrolls.NotFound",
            $"The payroll with the Id = '{payrollId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("Payrolls.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error PeriodCodeNotUnique = Error.Conflict(
        "Payrolls.PeriodCodeNotUnique",
        "The provided period code is not unique"
    );
}
