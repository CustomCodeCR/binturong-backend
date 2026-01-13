using SharedKernel;

namespace Domain.PurchaseOrders;

public static class PurchaseOrderErrors
{
    public static Error NotFound(Guid purchaseOrderId) =>
        Error.NotFound(
            "PurchaseOrders.NotFound",
            $"The purchase order with the Id = '{purchaseOrderId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "PurchaseOrders.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error CodeNotUnique = Error.Conflict(
        "PurchaseOrders.CodeNotUnique",
        "The provided purchase order code is not unique"
    );
}
