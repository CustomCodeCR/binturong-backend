using SharedKernel;

namespace Domain.CreditNotes;

public static class CreditNoteErrors
{
    public static readonly Error InvoiceRequired = Error.Validation(
        "CreditNotes.InvoiceRequired",
        "InvoiceId is required."
    );

    public static readonly Error ReasonRequired = Error.Validation(
        "CreditNotes.ReasonRequired",
        "Reason is required."
    );

    public static readonly Error TotalAmountInvalid = Error.Validation(
        "CreditNotes.TotalAmountInvalid",
        "TotalAmount must be > 0."
    );

    public static readonly Error ReasonNotAuthorized = Error.Validation(
        "CreditNotes.ReasonNotAuthorized",
        "Reason is not authorized."
    );

    public static Error InvoiceNotFound(Guid invoiceId) =>
        Error.NotFound("Invoices.NotFound", $"Invoice '{invoiceId}' not found.");

    public static readonly Error InvoiceNotEmitted = Error.Validation(
        "CreditNotes.InvoiceNotEmitted",
        "Invoice must be emitted before creating a credit note."
    );

    public static Error NotFound(Guid id) =>
        Error.NotFound("CreditNotes.NotFound", $"Credit note '{id}' not found.");
}
