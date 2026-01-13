using SharedKernel;

namespace Domain.ServiceOrders;

public static class ServiceOrderErrors
{
    public static Error NotFound(Guid serviceOrderId) =>
        Error.NotFound(
            "ServiceOrders.NotFound",
            $"The service order with the Id = '{serviceOrderId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "ServiceOrders.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error CodeNotUnique = Error.Conflict(
        "ServiceOrders.CodeNotUnique",
        "The provided service order code is not unique"
    );
}
