namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string InventoryMovementsCreate = "inventory.movements.create";

    public const string InventoryTransfersRead = "inventory.transfers.read";
    public const string InventoryTransfersCreate = "inventory.transfers.create";
    public const string InventoryTransfersUpdate = "inventory.transfers.update";
    public const string InventoryTransfersDelete = "inventory.transfers.delete";

    public const string InventoryTransfersRequestReview = "inventory.transfers.request_review";
    public const string InventoryTransfersApprove = "inventory.transfers.approve";
    public const string InventoryTransfersReject = "inventory.transfers.reject";
    public const string InventoryTransfersConfirm = "inventory.transfers.confirm";
    public const string InventoryTransfersCancel = "inventory.transfers.cancel";

    public const string InventoryByBranchRead = "inventory.by_branch.read";
    public const string InventoryStockRead = "inventory.stock.read";
}
