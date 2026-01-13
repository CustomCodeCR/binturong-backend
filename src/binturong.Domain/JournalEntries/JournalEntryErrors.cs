using SharedKernel;

namespace Domain.JournalEntries;

public static class JournalEntryErrors
{
    public static Error NotFound(Guid journalEntryId) =>
        Error.NotFound(
            "JournalEntries.NotFound",
            $"The journal entry with the Id = '{journalEntryId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "JournalEntries.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error NumberNotUnique = Error.Conflict(
        "JournalEntries.NumberNotUnique",
        "The provided journal entry number is not unique"
    );
}
