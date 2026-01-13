using SharedKernel;

namespace Domain.PurchaseReceiptDetails;

public static class PurchaseReceiptDetailErrors
{
    public static Error NotFound(Guid receiptDetailId) =>
        Error.NotFound(
            "PurchaseReceiptDetails.NotFound",
            $"The purchase receipt detail with the Id = '{receiptDetailId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "PurchaseReceiptDetails.Unauthorized",
            "You are not authorized to perform this action."
        );
}
