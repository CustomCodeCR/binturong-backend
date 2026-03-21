namespace Api.Endpoints.Reports;

public sealed record CreateReportScheduleRequest(
    string Name,
    string ReportType,
    string Frequency,
    string RecipientEmail,
    TimeSpan TimeOfDayUtc,
    bool IsActive,
    Guid? BranchId,
    Guid? CategoryId,
    Guid? ClientId,
    Guid? EmployeeId
);

public sealed record UpdateReportScheduleRequest(
    string Name,
    string ReportType,
    string Frequency,
    string RecipientEmail,
    TimeSpan TimeOfDayUtc,
    bool IsActive,
    Guid? BranchId,
    Guid? CategoryId,
    Guid? ClientId,
    Guid? EmployeeId
);
