using SharedKernel;

namespace Domain.DebitNotes;

public static class DebitNoteErrors
{
    public static Error NotFound(Guid debitNoteId) =>
        Error.NotFound(
            "DebitNotes.NotFound",
            $"The debit note with the Id = '{debitNoteId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("DebitNotes.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error ConsecutiveNotUnique = Error.Conflict(
        "DebitNotes.ConsecutiveNotUnique",
        "The provided consecutive is not unique"
    );
}
