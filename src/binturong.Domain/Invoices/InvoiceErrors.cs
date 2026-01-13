using SharedKernel;

namespace Domain.Invoices;

public static class InvoiceErrors
{
    public static Error NotFound(Guid invoiceId) =>
        Error.NotFound(
            "Invoices.NotFound",
            $"The invoice with the Id = '{invoiceId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("Invoices.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error ConsecutiveNotUnique = Error.Conflict(
        "Invoices.ConsecutiveNotUnique",
        "The provided consecutive is not unique"
    );

    public static readonly Error TaxKeyNotUnique = Error.Conflict(
        "Invoices.TaxKeyNotUnique",
        "The provided tax key is not unique"
    );
}
