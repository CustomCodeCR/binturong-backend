using SharedKernel;

namespace Domain.PaymentDetails;

public static class PaymentDetailErrors
{
    public static Error NotFound(Guid paymentDetailId) =>
        Error.NotFound(
            "PaymentDetails.NotFound",
            $"The payment detail with the Id = '{paymentDetailId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "PaymentDetails.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error InvalidAppliedAmount = Error.Validation(
        "PaymentDetails.InvalidAppliedAmount",
        "Applied amount must be greater than zero"
    );
}
