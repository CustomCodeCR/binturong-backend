using SharedKernel;

namespace Domain.PurchaseOrderDetails;

public static class PurchaseOrderDetailErrors
{
    public static Error NotFound(Guid purchaseOrderDetailId) =>
        Error.NotFound(
            "PurchaseOrderDetails.NotFound",
            $"The purchase order detail with the Id = '{purchaseOrderDetailId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "PurchaseOrderDetails.Unauthorized",
            "You are not authorized to perform this action."
        );
}
