namespace Api.Endpoints.Payroll;

public sealed record CreatePayrollRequest(
    string PeriodCode,
    DateOnly StartDate,
    DateOnly EndDate,
    string PayrollType
);

public sealed record UpdatePayrollRequest(
    string PeriodCode,
    DateOnly StartDate,
    DateOnly EndDate,
    string PayrollType,
    string Status
);

public sealed record CalculatePayrollRequest(
    string PeriodCode,
    DateOnly StartDate,
    DateOnly EndDate,
    string PayrollType,
    bool AttendanceConfirmed
);

public sealed record RegisterOvertimeRequest(
    Guid EmployeeId,
    DateOnly WorkDate,
    decimal Hours,
    string? Notes
);

public sealed record AdjustCommissionRequest(decimal CommissionAmount);
