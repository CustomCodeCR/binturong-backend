using SharedKernel;

namespace Domain.Payments;

public static class PaymentErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Payments.NotFound", $"Payment '{id}' not found.");

    public static readonly Error ClientRequired = Error.Validation(
        "Payments.ClientRequired",
        "ClientId is required."
    );

    public static readonly Error PaymentMethodRequired = Error.Validation(
        "Payments.PaymentMethodRequired",
        "PaymentMethodId is required."
    );

    public static readonly Error NoDetails = Error.Validation(
        "Payments.NoDetails",
        "Payment must have at least one applied invoice."
    );

    public static readonly Error AppliedAmountInvalid = Error.Validation(
        "Payments.AppliedAmountInvalid",
        "AppliedAmount must be > 0."
    );

    public static Error MethodRequired =>
        Error.Validation("Payments.MethodRequired", "PaymentMethodId is required.");

    public static Error AmountInvalid =>
        Error.Validation("Payments.AmountInvalid", "TotalAmount must be greater than 0.");

    public static Error ReferenceRequired =>
        Error.Validation("Payments.ReferenceRequired", "Reference is required.");

    public static Error PosRejected =>
        Error.Validation("Payments.PosRejected", "POS transaction was rejected.");

    public static Error PosUnavailable =>
        Error.Validation("Payments.PosUnavailable", "POS system is unavailable.");

    public static Error NoAppliedInvoices =>
        Error.Validation("Payments.NoAppliedInvoices", "At least one invoice must be applied.");
}
