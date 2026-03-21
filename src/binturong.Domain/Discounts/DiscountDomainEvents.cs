using SharedKernel;

namespace Domain.Discounts;

// Policy
public sealed record DiscountPolicyCreatedDomainEvent(
    Guid PolicyId,
    string Name,
    decimal MaxDiscountPercentage,
    bool RequiresApprovalAboveLimit,
    bool IsActive
) : IDomainEvent;

public sealed record DiscountPolicyUpdatedDomainEvent(
    Guid PolicyId,
    string Name,
    decimal MaxDiscountPercentage,
    bool RequiresApprovalAboveLimit,
    bool IsActive,
    DateTime UpdatedAtUtc
) : IDomainEvent;

public sealed record DiscountPolicyDeletedDomainEvent(Guid PolicyId) : IDomainEvent;

// Approval workflow
public sealed record DiscountApprovalRequestedDomainEvent(
    Guid ApprovalRequestId,
    Guid SalesOrderId,
    Guid? SalesOrderDetailId,
    string Scope, // Line | Total
    decimal RequestedPercentage,
    decimal RequestedAmount,
    string Reason,
    Guid RequestedByUserId,
    DateTime RequestedAtUtc
) : IDomainEvent;

public sealed record DiscountApprovalApprovedDomainEvent(
    Guid ApprovalRequestId,
    Guid ApprovedByUserId,
    DateTime ApprovedAtUtc
) : IDomainEvent;

public sealed record DiscountApprovalRejectedDomainEvent(
    Guid ApprovalRequestId,
    Guid RejectedByUserId,
    string RejectionReason,
    DateTime RejectedAtUtc
) : IDomainEvent;

// Sales order / line discounts
public sealed record SalesOrderLineDiscountAppliedDomainEvent(
    Guid SalesOrderId,
    Guid SalesOrderDetailId,
    decimal DiscountPerc,
    decimal DiscountAmount,
    string Reason,
    Guid AppliedByUserId,
    DateTime AppliedAtUtc
) : IDomainEvent;

public sealed record SalesOrderLineDiscountRemovedDomainEvent(
    Guid SalesOrderId,
    Guid SalesOrderDetailId,
    Guid RemovedByUserId,
    DateTime RemovedAtUtc
) : IDomainEvent;

public sealed record SalesOrderGlobalDiscountAppliedDomainEvent(
    Guid SalesOrderId,
    decimal DiscountPerc,
    decimal DiscountAmount,
    string Reason,
    Guid AppliedByUserId,
    DateTime AppliedAtUtc
) : IDomainEvent;

public sealed record SalesOrderGlobalDiscountRemovedDomainEvent(
    Guid SalesOrderId,
    Guid RemovedByUserId,
    DateTime RemovedAtUtc
) : IDomainEvent;
