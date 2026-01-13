using SharedKernel;

namespace Domain.CreditNotes;

public static class CreditNoteErrors
{
    public static Error NotFound(Guid creditNoteId) =>
        Error.NotFound(
            "CreditNotes.NotFound",
            $"The credit note with the Id = '{creditNoteId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("CreditNotes.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error ConsecutiveNotUnique = Error.Conflict(
        "CreditNotes.ConsecutiveNotUnique",
        "The provided consecutive is not unique"
    );
}
