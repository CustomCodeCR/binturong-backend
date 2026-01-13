using SharedKernel;

namespace Domain.PaymentGatewayConfig;

public static class PaymentGatewayConfigurationErrors
{
    public static Error NotFound(Guid gatewayId) =>
        Error.NotFound(
            "PaymentGatewayConfig.NotFound",
            $"The payment gateway configuration with the Id = '{gatewayId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "PaymentGatewayConfig.Unauthorized",
            "You are not authorized to perform this action."
        );
}
