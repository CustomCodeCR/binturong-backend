namespace Api.Endpoints.Discounts;

public sealed record CreateDiscountPolicyRequest(
    string Name,
    decimal MaxDiscountPercentage,
    bool RequiresApprovalAboveLimit,
    bool IsActive
);

public sealed record UpdateDiscountPolicyRequest(
    string Name,
    decimal MaxDiscountPercentage,
    bool RequiresApprovalAboveLimit,
    bool IsActive
);

public sealed record ApplyLineDiscountRequest(
    Guid SalesOrderId,
    Guid SalesOrderDetailId,
    decimal DiscountPerc,
    string Reason,
    Guid PolicyId
);

public sealed record ApplyApprovedLineDiscountRequest(
    Guid SalesOrderId,
    Guid SalesOrderDetailId,
    decimal DiscountPerc,
    string Reason,
    Guid ApprovalRequestId
);

public sealed record RemoveLineDiscountRequest(Guid SalesOrderId, Guid SalesOrderDetailId);

public sealed record ApplyGlobalDiscountRequest(
    Guid SalesOrderId,
    decimal DiscountPerc,
    string Reason,
    Guid PolicyId
);

public sealed record ApplyApprovedGlobalDiscountRequest(
    Guid SalesOrderId,
    decimal DiscountPerc,
    string Reason,
    Guid ApprovalRequestId
);

public sealed record RemoveGlobalDiscountRequest(Guid SalesOrderId);

public sealed record RequestLineDiscountApprovalRequest(
    Guid SalesOrderId,
    Guid SalesOrderDetailId,
    decimal DiscountPerc,
    string Reason
);

public sealed record RequestGlobalDiscountApprovalRequest(
    Guid SalesOrderId,
    decimal DiscountPerc,
    string Reason
);

public sealed record RejectDiscountApprovalRequest(string RejectionReason);
