using SharedKernel;

namespace Domain.Employees;

public static class EmployeeErrors
{
    public static Error NotFound(Guid employeeId) =>
        Error.NotFound("Employees.NotFound", $"Employee with Id '{employeeId}' was not found");

    public static readonly Error NationalIdNotUnique = Error.Conflict(
        "Employees.NationalIdNotUnique",
        "The provided national id is already in use"
    );

    public static readonly Error EmployeeInactive = Error.Failure(
        "Employees.Inactive",
        "The employee is not active"
    );

    public static readonly Error AttendanceAlreadyOpen = Error.Failure(
        "Employees.AttendanceAlreadyOpen",
        "The employee already has an open attendance record"
    );

    public static readonly Error AttendanceNotOpen = Error.Failure(
        "Employees.AttendanceNotOpen",
        "There is no open attendance record for the employee"
    );
}
