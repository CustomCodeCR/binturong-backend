namespace Application.ReadModels.Discounts;

public sealed class DiscountApprovalRequestReadModel
{
    public string Id { get; init; } = default!; // "discount_approval:{RequestId}"
    public Guid ApprovalRequestId { get; init; }

    public Guid SalesOrderId { get; init; }
    public string? SalesOrderCode { get; init; }

    public Guid? SalesOrderDetailId { get; init; }

    public string Scope { get; init; } = default!; // Line | Total

    public decimal RequestedPercentage { get; init; }
    public decimal RequestedAmount { get; init; }

    public string Reason { get; init; } = default!;

    public Guid RequestedByUserId { get; init; }
    public string? RequestedByUserName { get; init; }
    public DateTime RequestedAtUtc { get; init; }

    public string Status { get; init; } = default!; // Pending | Approved | Rejected

    public Guid? ResolvedByUserId { get; init; }
    public string? ResolvedByUserName { get; init; }
    public DateTime? ResolvedAtUtc { get; init; }
    public string? RejectionReason { get; init; }
}
