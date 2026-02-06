namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string EmployeesRead = "employees.read";
    public const string EmployeesCreate = "employees.create";
    public const string EmployeesUpdate = "employees.update";
    public const string EmployeesDelete = "employees.delete";

    public const string EmployeesAttendanceCheckIn = "employees.attendance.checkin";
    public const string EmployeesAttendanceCheckOut = "employees.attendance.checkout";
}
