using SharedKernel;

namespace Domain.InventoryTransfers;

public sealed class InventoryTransfer : Entity
{
    public Guid Id { get; set; }

    public Guid FromBranchId { get; set; }
    public Guid ToBranchId { get; set; }

    public string Status { get; set; } = InventoryTransferStatus.Draft;

    public string Notes { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Domain.Branches.Branch? FromBranch { get; set; }
    public Domain.Branches.Branch? ToBranch { get; set; }

    public ICollection<InventoryTransferLine> Lines { get; set; } =
        new List<InventoryTransferLine>();

    // =========================
    // Domain events
    // =========================

    public void RaiseCreated() =>
        Raise(
            new InventoryTransferCreatedDomainEvent(
                TransferId: Id,
                FromBranchId: FromBranchId,
                ToBranchId: ToBranchId,
                Status: Status,
                Notes: Notes,
                CreatedByUserId: CreatedByUserId,
                ApprovedByUserId: ApprovedByUserId,
                RejectionReason: RejectionReason,
                CreatedAt: CreatedAt,
                UpdatedAt: UpdatedAt,
                Lines: Lines
                    .Select(l => new InventoryTransferLineSnapshot(
                        LineId: l.Id,
                        ProductId: l.ProductId,
                        FromWarehouseId: l.FromWarehouseId,
                        ToWarehouseId: l.ToWarehouseId,
                        Quantity: l.Quantity
                    ))
                    .ToList()
            )
        );

    // Used if you ever edit notes/lines while still Draft (optional)
    public void RaiseUpdated() =>
        Raise(new InventoryTransferUpdatedDomainEvent(Id, Notes, UpdatedAt));

    // ✅ REQUIRED by your handlers
    public void RaiseReviewRequested() =>
        Raise(new InventoryTransferReviewRequestedDomainEvent(Id, Status, UpdatedAt));

    // ✅ REQUIRED by your handlers
    public void RaiseApproved(Guid approvedByUserId) =>
        Raise(new InventoryTransferApprovedDomainEvent(Id, Status, approvedByUserId, UpdatedAt));

    // ✅ REQUIRED by your handlers
    public void RaiseRejected(Guid rejectedByUserId, string? reason) =>
        Raise(
            new InventoryTransferRejectedDomainEvent(
                Id,
                Status,
                rejectedByUserId,
                reason,
                UpdatedAt
            )
        );

    // ✅ REQUIRED by your handlers
    public void RaiseConfirmed() =>
        Raise(new InventoryTransferConfirmedDomainEvent(Id, Status, UpdatedAt));

    public void RaiseDeleted() => Raise(new InventoryTransferDeletedDomainEvent(Id));

    public void RaiseCancelled(Guid cancelledByUserId, string? reason) =>
        Raise(
            new InventoryTransferCancelledDomainEvent(
                Id,
                Status,
                cancelledByUserId,
                reason,
                UpdatedAt
            )
        );
}
