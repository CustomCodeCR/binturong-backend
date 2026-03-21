using SharedKernel;

namespace Domain.Reports;

public sealed record ReportScheduleCreatedDomainEvent(
    Guid ReportScheduleId,
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
) : IDomainEvent;

public sealed record ReportScheduleUpdatedDomainEvent(
    Guid ReportScheduleId,
    string Name,
    string ReportType,
    string Frequency,
    string RecipientEmail,
    TimeSpan TimeOfDayUtc,
    bool IsActive,
    Guid? BranchId,
    Guid? CategoryId,
    Guid? ClientId,
    Guid? EmployeeId,
    DateTime UpdatedAtUtc
) : IDomainEvent;

public sealed record ReportScheduleDeletedDomainEvent(Guid ReportScheduleId) : IDomainEvent;

public sealed record ReportScheduleExecutionSucceededDomainEvent(
    Guid ReportScheduleId,
    DateTime ExecutedAtUtc
) : IDomainEvent;

public sealed record ReportScheduleExecutionFailedDomainEvent(
    Guid ReportScheduleId,
    string ErrorMessage,
    DateTime FailedAtUtc
) : IDomainEvent;
