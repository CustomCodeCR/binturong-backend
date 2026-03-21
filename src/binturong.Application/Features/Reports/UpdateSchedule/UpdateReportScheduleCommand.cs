using Application.Abstractions.Messaging;

namespace Application.Features.Reports.UpdateSchedule;

public sealed record UpdateReportScheduleCommand(
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
) : ICommand;
