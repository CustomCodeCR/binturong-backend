using SharedKernel;

namespace Domain.Contracts;

public static class ContractErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Contracts.NotFound", $"Contract '{id}' not found.");

    public static Error ClientNotFound(Guid id) =>
        Error.NotFound("Clients.NotFound", $"Client '{id}' not found.");

    public static readonly Error ClientRequired = Error.Validation(
        "Contracts.ClientRequired",
        "ClientId is required."
    );

    public static readonly Error StartDateRequired = Error.Validation(
        "Contracts.StartDateRequired",
        "StartDate is required."
    );

    public static Error InvalidValidity(DateOnly startDate, DateOnly endDate) =>
        Error.Validation(
            "Contracts.InvalidValidity",
            $"EndDate '{endDate:yyyy-MM-dd}' cannot be earlier than StartDate '{startDate:yyyy-MM-dd}'."
        );

    public static readonly Error MilestoneDescriptionRequired = Error.Validation(
        "Contracts.Milestone.DescriptionRequired",
        "Milestone description is required."
    );

    public static readonly Error MilestonePercentageInvalid = Error.Validation(
        "Contracts.Milestone.PercentageInvalid",
        "Percentage must be between 0 and 100."
    );

    public static readonly Error MilestoneAmountInvalid = Error.Validation(
        "Contracts.Milestone.AmountInvalid",
        "Amount must be >= 0."
    );

    public static Error AttachmentInvalidFormat(string ext) =>
        Error.Validation(
            "Contracts.Attachments.InvalidFormat",
            $"File extension '{ext}' is not allowed."
        );

    public static readonly Error AttachmentMissing = Error.Validation(
        "Contracts.Attachments.Missing",
        "No file was provided."
    );

    public static readonly Error AttachmentNotFound = Error.NotFound(
        "Contracts.Attachments.NotFound",
        "Attachment not found."
    );

    public static readonly Error AttachmentTooLarge = Error.Validation(
        "Contracts.Attachments.TooLarge",
        "File is too large."
    );

    public static readonly Error QuoteRequired = Error.Validation(
        "Contracts.QuoteRequired",
        "QuoteId is required."
    );

    public static Error QuoteNotFound(Guid id) =>
        Error.NotFound("Quotes.NotFound", $"Quote '{id}' not found.");

    public static readonly Error QuoteNotAccepted = Error.Validation(
        "Contracts.QuoteNotAccepted",
        "Quote must be in status 'Accepted'."
    );

    public static readonly Error ContractAlreadyExistsForQuote = Error.Conflict(
        "Contracts.AlreadyExistsForQuote",
        "A contract already exists for this quote."
    );

    public static readonly Error ResponsibleRequired = Error.Validation(
        "Contracts.ResponsibleRequired",
        "ResponsibleUserId is required."
    );

    public static readonly Error AutoRenewEveryDaysInvalid = Error.Validation(
        "Contracts.AutoRenewEveryDaysInvalid",
        "AutoRenewEveryDays must be >= 1."
    );

    public static readonly Error ExpiryNoticeDaysInvalid = Error.Validation(
        "Contracts.ExpiryNoticeDaysInvalid",
        "ExpiryNoticeDays must be >= 0."
    );

    public static readonly Error ResponsibleUserRequired = Error.Validation(
        "Contracts.ResponsibleUserRequired",
        "ResponsibleUserId is required."
    );
}
