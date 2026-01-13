using SharedKernel;

namespace Domain.GatewayTransactions;

public static class GatewayTransactionErrors
{
    public static Error NotFound(Guid transactionId) =>
        Error.NotFound(
            "GatewayTransactions.NotFound",
            $"The gateway transaction with the Id = '{transactionId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "GatewayTransactions.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error GatewayReferenceNotUnique = Error.Conflict(
        "GatewayTransactions.GatewayReferenceNotUnique",
        "The provided gateway reference is not unique"
    );
}
