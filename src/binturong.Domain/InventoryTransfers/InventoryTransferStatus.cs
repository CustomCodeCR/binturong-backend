namespace Domain.InventoryTransfers;

public static class InventoryTransferStatus
{
    public const string Draft = "Draft";
    public const string PendingReview = "PendingReview"; // “En revisión”
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}
