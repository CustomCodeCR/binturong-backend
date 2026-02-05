using SharedKernel;

namespace Domain.InventoryTransfers;

public sealed record InventoryTransferLineSnapshot(
    Guid LineId,
    Guid ProductId,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    decimal Quantity
);

public sealed record InventoryTransferCreatedDomainEvent(
    Guid TransferId,
    Guid FromBranchId,
    Guid ToBranchId,
    string Status,
    string Notes,
    Guid CreatedByUserId,
    Guid? ApprovedByUserId,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<InventoryTransferLineSnapshot> Lines
) : IDomainEvent;

public sealed record InventoryTransferUpdatedDomainEvent(
    Guid TransferId,
    string Notes,
    DateTime UpdatedAt
) : IDomainEvent;

// ✅ Used by RequestReview handler
public sealed record InventoryTransferReviewRequestedDomainEvent(
    Guid TransferId,
    string Status,
    DateTime UpdatedAt
) : IDomainEvent;

// ✅ Used by Approve handler
public sealed record InventoryTransferApprovedDomainEvent(
    Guid TransferId,
    string Status,
    Guid ApprovedByUserId,
    DateTime UpdatedAt
) : IDomainEvent;

// ✅ Used by Reject handler
public sealed record InventoryTransferRejectedDomainEvent(
    Guid TransferId,
    string Status,
    Guid RejectedByUserId,
    string? Reason,
    DateTime UpdatedAt
) : IDomainEvent;

// ✅ Used by Confirm handler
public sealed record InventoryTransferConfirmedDomainEvent(
    Guid TransferId,
    string Status,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record InventoryTransferDeletedDomainEvent(Guid TransferId) : IDomainEvent;

public sealed record InventoryTransferCancelledDomainEvent(
    Guid TransferId,
    string Status,
    Guid CancelledByUserId,
    string? Reason,
    DateTime UpdatedAt
) : IDomainEvent;
