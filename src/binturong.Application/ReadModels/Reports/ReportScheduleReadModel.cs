namespace Application.ReadModels.Reports;

public sealed class ReportScheduleReadModel
{
    public string Id { get; init; } = default!; // "report_schedule:{Id}"
    public Guid ReportScheduleId { get; init; }

    public string Name { get; init; } = default!;
    public string ReportType { get; init; } = default!;
    public string Frequency { get; init; } = default!;
    public string RecipientEmail { get; init; } = default!;
    public TimeSpan TimeOfDayUtc { get; init; }

    public bool IsActive { get; init; }

    public Guid? BranchId { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? ClientId { get; init; }
    public Guid? EmployeeId { get; init; }

    public DateTime? LastSentAtUtc { get; init; }
    public DateTime? LastAttemptAtUtc { get; init; }
    public string? LastError { get; init; }
}
