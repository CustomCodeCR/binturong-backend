using System.Net.Mail;
using SharedKernel;

namespace Domain.Reports;

public sealed class ReportSchedule : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Financial | Inventory | ClientHistory | ServiceOrders
    public string ReportType { get; set; } = string.Empty;

    // Daily | Weekly | Monthly
    public string Frequency { get; set; } = string.Empty;

    public string RecipientEmail { get; set; } = string.Empty;
    public TimeSpan TimeOfDayUtc { get; set; }

    public bool IsActive { get; set; }

    public Guid? BranchId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? EmployeeId { get; set; }

    public DateTime? LastSentAtUtc { get; set; }
    public DateTime? LastAttemptAtUtc { get; set; }
    public string LastError { get; set; } = string.Empty;

    public void RaiseCreated() =>
        Raise(
            new ReportScheduleCreatedDomainEvent(
                Id,
                Name,
                ReportType,
                Frequency,
                RecipientEmail,
                TimeOfDayUtc,
                IsActive,
                BranchId,
                CategoryId,
                ClientId,
                EmployeeId
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new ReportScheduleUpdatedDomainEvent(
                Id,
                Name,
                ReportType,
                Frequency,
                RecipientEmail,
                TimeOfDayUtc,
                IsActive,
                BranchId,
                CategoryId,
                ClientId,
                EmployeeId,
                DateTime.UtcNow
            )
        );

    public void RaiseDeleted() => Raise(new ReportScheduleDeletedDomainEvent(Id));

    public Result Validate()
    {
        if (string.IsNullOrWhiteSpace(ReportType))
            return Result.Failure(ReportScheduleErrors.ReportTypeRequired);

        if (string.IsNullOrWhiteSpace(Frequency))
            return Result.Failure(ReportScheduleErrors.FrequencyRequired);

        if (string.IsNullOrWhiteSpace(RecipientEmail))
            return Result.Failure(ReportScheduleErrors.EmailRequired);

        var reportTypes = new[] { "Financial", "Inventory", "ClientHistory", "ServiceOrders" };
        if (!reportTypes.Contains(ReportType, StringComparer.OrdinalIgnoreCase))
            return Result.Failure(ReportScheduleErrors.InvalidReportType);

        var frequencies = new[] { "Daily", "Weekly", "Monthly" };
        if (!frequencies.Contains(Frequency, StringComparer.OrdinalIgnoreCase))
            return Result.Failure(ReportScheduleErrors.InvalidFrequency);

        if (TimeOfDayUtc < TimeSpan.Zero || TimeOfDayUtc >= TimeSpan.FromDays(1))
            return Result.Failure(ReportScheduleErrors.TimeOfDayInvalid);

        try
        {
            _ = new MailAddress(RecipientEmail);
        }
        catch
        {
            return Result.Failure(ReportScheduleErrors.RecipientEmailInvalid);
        }

        return Result.Success();
    }

    public bool ShouldRun(DateTime nowUtc)
    {
        if (!IsActive)
            return false;

        var scheduledToday = nowUtc.Date.Add(TimeOfDayUtc);

        if (nowUtc < scheduledToday)
            return false;

        return Frequency switch
        {
            "Daily" => LastSentAtUtc is null || LastSentAtUtc.Value.Date < nowUtc.Date,
            "Weekly" => LastSentAtUtc is null
                || (nowUtc.Date - LastSentAtUtc.Value.Date).TotalDays >= 7,
            "Monthly" => LastSentAtUtc is null
                || LastSentAtUtc.Value.Year != nowUtc.Year
                || LastSentAtUtc.Value.Month != nowUtc.Month,
            _ => false,
        };
    }

    public void MarkExecutionSucceeded(DateTime nowUtc)
    {
        LastAttemptAtUtc = nowUtc;
        LastSentAtUtc = nowUtc;
        LastError = string.Empty;

        Raise(new ReportScheduleExecutionSucceededDomainEvent(Id, nowUtc));
    }

    public void MarkExecutionFailed(string errorMessage, DateTime nowUtc)
    {
        LastAttemptAtUtc = nowUtc;
        LastError = errorMessage?.Trim() ?? string.Empty;

        Raise(new ReportScheduleExecutionFailedDomainEvent(Id, LastError, nowUtc));
    }
}
