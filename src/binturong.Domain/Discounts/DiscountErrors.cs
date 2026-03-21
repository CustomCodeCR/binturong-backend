using SharedKernel;

namespace Domain.Discounts;

public static class DiscountErrors
{
    public static readonly Error ReasonRequired = Error.Validation(
        "Discounts.ReasonRequired",
        "Discount reason is required."
    );

    public static readonly Error PercentageInvalid = Error.Validation(
        "Discounts.PercentageInvalid",
        "Discount percentage must be between 0 and 100."
    );

    public static readonly Error AmountInvalid = Error.Validation(
        "Discounts.AmountInvalid",
        "Discount amount must be greater than or equal to 0."
    );

    public static readonly Error MaxPercentageInvalid = Error.Validation(
        "Discounts.MaxPercentageInvalid",
        "Max discount percentage must be between 0 and 100."
    );

    public static readonly Error SalesOrderWithoutItems = Error.Validation(
        "Discounts.SalesOrderWithoutItems",
        "Cannot apply discount to an empty sales order."
    );

    public static readonly Error DiscountExceedsPolicy = Error.Validation(
        "Discounts.DiscountExceedsPolicy",
        "Discount exceeds the maximum allowed by policy."
    );

    public static readonly Error ApprovalRequestRequired = Error.Validation(
        "Discounts.ApprovalRequestRequired",
        "Discount requires approval before being applied."
    );

    public static readonly Error ApprovalRequestAlreadyResolved = Error.Validation(
        "Discounts.ApprovalRequestAlreadyResolved",
        "Discount approval request has already been resolved."
    );

    public static readonly Error ApprovalRequestRejected = Error.Validation(
        "Discounts.ApprovalRequestRejected",
        "Discount request was rejected and cannot be applied."
    );

    public static readonly Error ApprovalRequestNotApproved = Error.Validation(
        "Discounts.ApprovalRequestNotApproved",
        "Discount request is not approved."
    );

    public static readonly Error LineDiscountNotFound = Error.Validation(
        "Discounts.LineDiscountNotFound",
        "Line discount was not found."
    );

    public static readonly Error GlobalDiscountNotFound = Error.Validation(
        "Discounts.GlobalDiscountNotFound",
        "Global discount was not found."
    );

    public static readonly Error PolicyInactive = Error.Validation(
        "Discounts.PolicyInactive",
        "Discount policy is inactive."
    );

    public static Error PolicyNotFound(Guid policyId) =>
        Error.NotFound("Discounts.PolicyNotFound", $"Discount policy '{policyId}' not found.");

    public static Error ApprovalRequestNotFound(Guid requestId) =>
        Error.NotFound(
            "Discounts.ApprovalRequestNotFound",
            $"Discount approval request '{requestId}' not found."
        );
}
