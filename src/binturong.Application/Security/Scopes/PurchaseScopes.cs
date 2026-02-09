namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    // Purchase Requests
    public const string PurchaseRequestsRead = "purchase_requests.read";
    public const string PurchaseRequestsCreate = "purchase_requests.create";

    // Purchase Orders
    public const string PurchaseOrdersRead = "purchase_orders.read";
    public const string PurchaseOrdersCreate = "purchase_orders.create";

    // Purchase Receipts
    public const string PurchaseReceiptsRead = "purchase_receipts.read";
    public const string PurchaseReceiptsCreate = "purchase_receipts.create";
    public const string PurchaseReceiptsReject = "purchase_receipts.reject";
}
