namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    // Invoices
    public const string InvoicesRead = "invoices.read";
    public const string InvoicesCreate = "invoices.create";
    public const string InvoicesUpdate = "invoices.update";
    public const string InvoicesDelete = "invoices.delete";
    public const string InvoicesEmit = "invoices.emit";

    // Payments (A/R)
    public const string PaymentsRead = "payments.read";
    public const string PaymentsCreate = "payments.create";
    public const string PaymentsDelete = "payments.delete";
    public const string PaymentsExport = "payments.export";
    public const string PaymentsRegister = "payments.register";

    // Credit Notes
    public const string CreditNotesRead = "credit_notes.read";
    public const string CreditNotesCreate = "credit_notes.create";
    public const string CreditNotesDelete = "credit_notes.delete";
    public const string CreditNotesEmit = "credit_notes.emit";

    // Debit Notes
    public const string DebitNotesRead = "debit_notes.read";
    public const string DebitNotesCreate = "debit_notes.create";
    public const string DebitNotesDelete = "debit_notes.delete";
    public const string DebitNotesEmit = "debit_notes.emit";

    public const string InvoicesConvertFromQuote = "invoices.convert_from_quote";

    public const string AccountsReceivableRead = "accounts_receivable.read";
}
