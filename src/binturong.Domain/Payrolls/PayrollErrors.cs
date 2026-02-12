using SharedKernel;

namespace Application.Features.Payroll;

public static class PayrollErrors
{
    public static readonly Error PeriodCodeRequired = Error.Validation(
        "Payroll.PeriodCodeRequired",
        "PeriodCode is required."
    );

    public static readonly Error InvalidPeriod = Error.Validation(
        "Payroll.InvalidPeriod",
        "StartDate must be <= EndDate."
    );

    public static readonly Error AttendanceNotConfirmed = Error.Validation(
        "Payroll.AttendanceNotConfirmed",
        "Attendance must be confirmed before calculating payroll."
    );

    public static Error NotFound(Guid payrollId) =>
        Error.NotFound("Payroll.NotFound", $"Payroll '{payrollId}' not found.");

    public static readonly Error PayrollNotCalculated = Error.Validation(
        "Payroll.NotCalculated",
        "Payroll must be calculated first."
    );

    public static Error DetailNotFound(Guid detailId) =>
        Error.NotFound("Payroll.DetailNotFound", $"Payroll detail '{detailId}' not found.");

    public static readonly Error InvalidOvertimeHours = Error.Validation(
        "Payroll.Overtime.InvalidHours",
        "Overtime hours must be > 0."
    );

    public static readonly Error OvertimeTooHigh = Error.Validation(
        "Payroll.Overtime.TooHigh",
        "Overtime hours exceed maximum allowed."
    );

    public static readonly Error EmployeeEmailMissing = Error.Validation(
        "Payroll.Employee.EmailMissing",
        "Employee email is missing."
    );

    public static readonly Error EmployeeNotFound = Error.NotFound(
        "Employees.NotFound",
        "Employee not found."
    );
}
