using SharedKernel;

namespace Domain.DebitNotes;

public static class DebitNoteErrors
{
    public static readonly Error InvoiceRequired = Error.Validation(
        "DebitNotes.InvoiceRequired",
        "InvoiceId is required."
    );

    public static readonly Error ReasonRequired = Error.Validation(
        "DebitNotes.ReasonRequired",
        "Reason is required."
    );

    public static readonly Error TotalAmountInvalid = Error.Validation(
        "DebitNotes.TotalAmountInvalid",
        "TotalAmount must be > 0."
    );

    public static Error InvoiceNotFound(Guid invoiceId) =>
        Error.NotFound("Invoices.NotFound", $"Invoice '{invoiceId}' not found.");

    public static readonly Error InvoiceNotEmitted = Error.Validation(
        "DebitNotes.InvoiceNotEmitted",
        "Invoice must be emitted before creating a debit note."
    );

    public static Error NotFound(Guid id) =>
        Error.NotFound("DebitNotes.NotFound", $"Debit note '{id}' not found.");
}
