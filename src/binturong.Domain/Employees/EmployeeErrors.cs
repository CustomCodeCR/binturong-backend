using SharedKernel;

namespace Domain.Employees;

public static class EmployeeErrors
{
    public static Error NotFound(Guid employeeId) =>
        Error.NotFound(
            "Employees.NotFound",
            $"The employee with the Id = '{employeeId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("Employees.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error NationalIdNotUnique = Error.Conflict(
        "Employees.NationalIdNotUnique",
        "The provided national id is not unique"
    );
}
