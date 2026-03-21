using SharedKernel;

namespace Domain.Discounts;

public sealed class DiscountApprovalRequest : Entity
{
    public Guid Id { get; set; }

    public Guid SalesOrderId { get; set; }
    public Guid? SalesOrderDetailId { get; set; }

    // Line | Total
    public string Scope { get; set; } = string.Empty;

    public decimal RequestedPercentage { get; set; }
    public decimal RequestedAmount { get; set; }

    public string Reason { get; set; } = string.Empty;

    public Guid RequestedByUserId { get; set; }
    public DateTime RequestedAtUtc { get; set; }

    // Pending | Approved | Rejected
    public string Status { get; set; } = "Pending";

    public Guid? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public string RejectionReason { get; set; } = string.Empty;

    public void RaiseCreated() =>
        Raise(
            new DiscountApprovalRequestedDomainEvent(
                Id,
                SalesOrderId,
                SalesOrderDetailId,
                Scope,
                RequestedPercentage,
                RequestedAmount,
                Reason,
                RequestedByUserId,
                RequestedAtUtc
            )
        );

    public Result Approve(Guid approvedByUserId, DateTime approvedAtUtc)
    {
        if (Status != "Pending")
            return Result.Failure(DiscountErrors.ApprovalRequestAlreadyResolved);

        Status = "Approved";
        ResolvedByUserId = approvedByUserId;
        ResolvedAtUtc = approvedAtUtc;
        RejectionReason = string.Empty;

        Raise(new DiscountApprovalApprovedDomainEvent(Id, approvedByUserId, approvedAtUtc));
        return Result.Success();
    }

    public Result Reject(Guid rejectedByUserId, string rejectionReason, DateTime rejectedAtUtc)
    {
        if (Status != "Pending")
            return Result.Failure(DiscountErrors.ApprovalRequestAlreadyResolved);

        Status = "Rejected";
        ResolvedByUserId = rejectedByUserId;
        ResolvedAtUtc = rejectedAtUtc;
        RejectionReason = rejectionReason?.Trim() ?? string.Empty;

        Raise(
            new DiscountApprovalRejectedDomainEvent(
                Id,
                rejectedByUserId,
                RejectionReason,
                rejectedAtUtc
            )
        );

        return Result.Success();
    }

    public bool IsApproved() =>
        string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase);

    public bool IsRejected() =>
        string.Equals(Status, "Rejected", StringComparison.OrdinalIgnoreCase);

    public bool IsPending() => string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase);
}
