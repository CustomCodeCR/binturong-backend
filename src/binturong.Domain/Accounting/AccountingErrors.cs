using SharedKernel;

namespace Domain.Accounting;

public static class AccountingErrors
{
    public static readonly Error EntryTypeRequired = Error.Validation(
        "Accounting.EntryTypeRequired",
        "EntryType is required."
    );

    public static readonly Error AmountInvalid = Error.Validation(
        "Accounting.AmountInvalid",
        "Amount must be greater than 0."
    );

    public static readonly Error DetailRequired = Error.Validation(
        "Accounting.DetailRequired",
        "Detail is required."
    );

    public static readonly Error ClientRequired = Error.Validation(
        "Accounting.ClientRequired",
        "ClientId is required for income entries."
    );

    public static readonly Error SupplierRequired = Error.Validation(
        "Accounting.SupplierRequired",
        "SupplierId is required for expense entries."
    );

    public static readonly Error CategoryRequired = Error.Validation(
        "Accounting.CategoryRequired",
        "Category is required."
    );

    public static readonly Error DuplicateIncome = Error.Validation(
        "Accounting.DuplicateIncome",
        "An income entry with the same date and invoice number already exists."
    );

    public static readonly Error InvoiceNumberRequired = Error.Validation(
        "Accounting.InvoiceNumberRequired",
        "InvoiceNumber is required."
    );

    public static readonly Error EntryNotReconcilable = Error.Validation(
        "Accounting.EntryNotReconcilable",
        "Entry cannot be reconciled."
    );

    public static readonly Error AlreadyReconciled = Error.Validation(
        "Accounting.AlreadyReconciled",
        "Entry is already reconciled."
    );

    public static readonly Error DateRangeInvalid = Error.Validation(
        "Accounting.DateRangeInvalid",
        "FromUtc must be less than or equal to ToUtc."
    );

    public static Error EntryNotFound(Guid id) =>
        Error.NotFound("Accounting.EntryNotFound", $"Accounting entry '{id}' not found.");

    public static Error ReconciliationNotFound(Guid id) =>
        Error.NotFound("Accounting.ReconciliationNotFound", $"Reconciliation '{id}' not found.");
}
