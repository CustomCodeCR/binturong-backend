using Application.Abstractions.Messaging;

namespace Application.Features.Reports.CreateSchedule;

public sealed record CreateReportScheduleCommand(
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
) : ICommand<Guid>;
