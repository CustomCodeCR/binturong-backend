using SharedKernel;

namespace Domain.AccountsPayable;

public static class AccountPayableErrors
{
    public static readonly Error SupplierRequired = Error.Validation(
        "AccountsPayable.SupplierRequired",
        "SupplierId is required."
    );

    public static readonly Error CurrencyRequired = Error.Validation(
        "AccountsPayable.CurrencyRequired",
        "Currency is required."
    );

    public static readonly Error TotalAmountInvalid = Error.Validation(
        "AccountsPayable.TotalAmountInvalid",
        "TotalAmount must be greater than zero."
    );

    public static readonly Error PendingBalanceInvalid = Error.Validation(
        "AccountsPayable.PendingBalanceInvalid",
        "PendingBalance must be greater than or equal to zero."
    );

    public static readonly Error DueDateInvalid = Error.Validation(
        "AccountsPayable.DueDateInvalid",
        "DueDate must be on or after DocumentDate."
    );

    public static Error NotFound(Guid id) =>
        Error.NotFound("AccountsPayable.NotFound", $"Accounts payable '{id}' not found.");

    public static readonly Error PaymentAmountInvalid = Error.Validation(
        "AccountsPayable.PaymentAmountInvalid",
        "Payment amount must be greater than zero."
    );

    public static readonly Error AlreadySettled = Error.Validation(
        "AccountsPayable.AlreadySettled",
        "Account payable is already settled."
    );

    public static readonly Error PaymentExceedsBalance = Error.Validation(
        "AccountsPayable.PaymentExceedsBalance",
        "Payment amount exceeds pending balance."
    );
}
