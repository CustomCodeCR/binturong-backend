using SharedKernel;

namespace Domain.Reports;

public static class ReportScheduleErrors
{
    public static readonly Error ReportTypeRequired = Error.Validation(
        "ReportSchedules.ReportTypeRequired",
        "ReportType is required."
    );

    public static readonly Error FrequencyRequired = Error.Validation(
        "ReportSchedules.FrequencyRequired",
        "Frequency is required."
    );

    public static readonly Error EmailRequired = Error.Validation(
        "ReportSchedules.EmailRequired",
        "Email is required."
    );

    public static readonly Error InvalidFrequency = Error.Validation(
        "ReportSchedules.InvalidFrequency",
        "Frequency must be Daily, Weekly, or Monthly."
    );

    public static readonly Error InvalidReportType = Error.Validation(
        "ReportSchedules.InvalidReportType",
        "ReportType must be Financial, Inventory, ClientHistory, or ServiceOrders."
    );

    public static readonly Error TimeOfDayInvalid = Error.Validation(
        "ReportSchedules.TimeOfDayInvalid",
        "TimeOfDayUtc must be between 00:00:00 and 23:59:59."
    );

    public static readonly Error RecipientEmailInvalid = Error.Validation(
        "ReportSchedules.RecipientEmailInvalid",
        "Recipient email is invalid."
    );

    public static Error NotFound(Guid id) =>
        Error.NotFound("ReportSchedules.NotFound", $"Report schedule '{id}' not found.");
}
