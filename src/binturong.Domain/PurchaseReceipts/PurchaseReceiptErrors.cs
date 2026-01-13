using SharedKernel;

namespace Domain.PurchaseReceipts;

public static class PurchaseReceiptErrors
{
    public static Error NotFound(Guid receiptId) =>
        Error.NotFound(
            "PurchaseReceipts.NotFound",
            $"The purchase receipt with the Id = '{receiptId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "PurchaseReceipts.Unauthorized",
            "You are not authorized to perform this action."
        );
}
