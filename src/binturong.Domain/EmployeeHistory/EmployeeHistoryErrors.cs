using SharedKernel;

namespace Domain.EmployeeHistory;

public static class EmployeeHistoryErrors
{
    public static Error NotFound(Guid historyId) =>
        Error.NotFound(
            "EmployeeHistory.NotFound",
            $"The employee history entry with the Id = '{historyId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "EmployeeHistory.Unauthorized",
            "You are not authorized to perform this action."
        );
}
