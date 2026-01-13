using SharedKernel;

namespace Domain.Payments;

public static class PaymentErrors
{
    public static Error NotFound(Guid paymentId) =>
        Error.NotFound(
            "Payments.NotFound",
            $"The payment with the Id = '{paymentId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("Payments.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error InvalidAmount = Error.Validation(
        "Payments.InvalidAmount",
        "Payment total amount must be greater than zero"
    );
}
