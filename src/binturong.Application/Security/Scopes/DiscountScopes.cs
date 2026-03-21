namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string DiscountsPoliciesRead = "discounts.policies.read";
    public const string DiscountsPoliciesCreate = "discounts.policies.create";
    public const string DiscountsPoliciesUpdate = "discounts.policies.update";

    public const string DiscountsApprovalRead = "discounts.approval.read";
    public const string DiscountsApprovalRequest = "discounts.approval.request";
    public const string DiscountsApprovalApprove = "discounts.approval.approve";
    public const string DiscountsApprovalReject = "discounts.approval.reject";

    public const string DiscountsApply = "discounts.apply";
    public const string DiscountsRemove = "discounts.remove";

    public const string DiscountsHistoryRead = "discounts.history.read";
    public const string DiscountsHistoryExport = "discounts.history.export";
}
