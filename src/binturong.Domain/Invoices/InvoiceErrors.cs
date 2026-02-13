using SharedKernel;

namespace Domain.Invoices;

public static class InvoiceErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Invoices.NotFound", $"Invoice '{id}' not found.");

    public static readonly Error ClientRequired = Error.Validation(
        "Invoices.ClientRequired",
        "ClientId is required."
    );

    public static readonly Error NoLines = Error.Validation(
        "Invoices.NoLines",
        "Invoice must contain at least one line."
    );

    public static Error MissingTaxField(string field) =>
        Error.Validation("Invoices.Tax.MissingField", $"Missing required tax field: {field}");

    public static readonly Error HaciendaUnavailable = Error.Failure(
        "Invoices.Hacienda.Unavailable",
        "Hacienda service is unavailable."
    );

    public static readonly Error QuoteNotAccepted = Error.Validation(
        "Invoices.Quote.NotAccepted",
        "Quote must be Accepted to convert."
    );

    public static readonly Error InventoryInsufficient = Error.Validation(
        "Invoices.Inventory.Insufficient",
        "Insufficient inventory."
    );

    public static readonly Error ClientCreditOverdue = Error.Validation(
        "Invoices.Credit.Overdue",
        "Client has overdue receivables."
    );

    public static readonly Error InvalidAmount = Error.Validation(
        "Invoices.Amount.Invalid",
        "Amount must be greater than zero."
    );

    public static Error NotEmitted(Guid invoiceId) =>
        Error.Validation("Invoices.NotEmitted", $"Invoice '{invoiceId}' is not emitted.");

    public static Error AlreadyPaid(Guid invoiceId) =>
        Error.Validation("Invoices.AlreadyPaid", $"Invoice '{invoiceId}' is already paid.");

    public static Error PaymentAmountInvalid =>
        Error.Validation("Invoices.PaymentAmountInvalid", "Payment amount must be greater than 0.");

    public static Error PaymentExceedsPending =>
        Error.Validation(
            "Invoices.PaymentExceedsPending",
            "Payment exceeds the pending balance for the invoice."
        );

    public static Error PaymentInsufficientToSettle =>
        Error.Validation(
            "Invoices.PaymentInsufficientToSettle",
            "Payment is insufficient to settle the invoice."
        );
}
