using SharedKernel;

namespace Domain.JournalEntryDetails;

public static class JournalEntryDetailErrors
{
    public static Error NotFound(Guid journalEntryDetailId) =>
        Error.NotFound(
            "JournalEntryDetails.NotFound",
            $"The journal entry detail with the Id = '{journalEntryDetailId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "JournalEntryDetails.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error DebitAndCreditInvalid = Error.Validation(
        "JournalEntryDetails.DebitAndCreditInvalid",
        "Debit and Credit cannot both be greater than zero at the same time"
    );
}
