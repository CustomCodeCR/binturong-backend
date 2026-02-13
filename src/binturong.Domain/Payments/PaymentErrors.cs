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
}
