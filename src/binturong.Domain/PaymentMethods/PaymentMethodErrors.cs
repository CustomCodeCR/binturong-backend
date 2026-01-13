using SharedKernel;

namespace Domain.PaymentMethods;

public static class PaymentMethodErrors
{
    public static Error NotFound(Guid paymentMethodId) =>
        Error.NotFound(
            "PaymentMethods.NotFound",
            $"The payment method with the Id = '{paymentMethodId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "PaymentMethods.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error CodeNotUnique = Error.Conflict(
        "PaymentMethods.CodeNotUnique",
        "The provided payment method code is not unique"
    );
}
