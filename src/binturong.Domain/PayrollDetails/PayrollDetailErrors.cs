using SharedKernel;

namespace Domain.PayrollDetails;

public static class PayrollDetailErrors
{
    public static Error NotFound(Guid payrollDetailId) =>
        Error.NotFound(
            "PayrollDetails.NotFound",
            $"The payroll detail with the Id = '{payrollDetailId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "PayrollDetails.Unauthorized",
            "You are not authorized to perform this action."
        );
}
