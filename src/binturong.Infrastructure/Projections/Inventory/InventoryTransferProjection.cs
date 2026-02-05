using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using Domain.InventoryTransfers;
using MongoDB.Driver;

namespace Infrastructure.Projections.Inventory;

internal sealed class InventoryTransferProjection
    : IProjector<InventoryTransferCreatedDomainEvent>,
        IProjector<InventoryTransferUpdatedDomainEvent>,
        IProjector<InventoryTransferDeletedDomainEvent>,
        IProjector<InventoryTransferReviewRequestedDomainEvent>,
        IProjector<InventoryTransferApprovedDomainEvent>,
        IProjector<InventoryTransferRejectedDomainEvent>,
        IProjector<InventoryTransferConfirmedDomainEvent>,
        IProjector<InventoryTransferCancelledDomainEvent>
{
    private readonly IMongoDatabase _db;

    public InventoryTransferProjection(IMongoDatabase db) => _db = db;

    // =========================
    // CREATED (full upsert)
    // =========================
    public Task ProjectAsync(InventoryTransferCreatedDomainEvent e, CancellationToken ct) =>
        UpsertCreatedAsync(
            transferId: e.TransferId,
            fromBranchId: e.FromBranchId,
            toBranchId: e.ToBranchId,
            status: e.Status,
            notes: e.Notes,
            createdByUserId: e.CreatedByUserId,
            approvedByUserId: e.ApprovedByUserId,
            rejectionReason: e.RejectionReason,
            lines: e.Lines,
            createdAt: e.CreatedAt,
            updatedAt: e.UpdatedAt,
            ct
        );

    // =========================
    // UPDATED (partial)
    // Event only has Notes + UpdatedAt
    // =========================
    public async Task ProjectAsync(InventoryTransferUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InventoryTransferReadModel>(
            MongoCollections.InventoryTransfers
        );
        var id = $"transfer:{e.TransferId}";

        var update = Builders<InventoryTransferReadModel>
            .Update.Set(x => x.Notes, e.Notes)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }

    // =========================
    // DELETED
    // =========================
    public async Task ProjectAsync(InventoryTransferDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InventoryTransferReadModel>(
            MongoCollections.InventoryTransfers
        );
        await col.DeleteOneAsync(x => x.Id == $"transfer:{e.TransferId}", ct);
    }

    // =========================
    // STATUS CHANGES
    // =========================
    public Task ProjectAsync(InventoryTransferReviewRequestedDomainEvent e, CancellationToken ct) =>
        SetStatusAsync(
            transferId: e.TransferId,
            status: InventoryTransferStatus.PendingReview,
            approvedByUserId: null,
            rejectionReason: null,
            updatedAt: e.UpdatedAt,
            ct
        );

    public Task ProjectAsync(InventoryTransferApprovedDomainEvent e, CancellationToken ct) =>
        SetStatusAsync(
            transferId: e.TransferId,
            status: InventoryTransferStatus.Approved,
            approvedByUserId: e.ApprovedByUserId,
            rejectionReason: null,
            updatedAt: e.UpdatedAt,
            ct
        );

    public Task ProjectAsync(InventoryTransferRejectedDomainEvent e, CancellationToken ct) =>
        SetStatusAsync(
            transferId: e.TransferId,
            status: InventoryTransferStatus.Rejected,
            approvedByUserId: null,
            rejectionReason: e.Reason,
            updatedAt: e.UpdatedAt,
            ct
        );

    public Task ProjectAsync(InventoryTransferConfirmedDomainEvent e, CancellationToken ct) =>
        SetStatusAsync(
            transferId: e.TransferId,
            status: InventoryTransferStatus.Completed,
            approvedByUserId: null,
            rejectionReason: null,
            updatedAt: e.UpdatedAt,
            ct
        );

    public Task ProjectAsync(InventoryTransferCancelledDomainEvent e, CancellationToken ct) =>
        SetStatusAsync(
            transferId: e.TransferId,
            status: InventoryTransferStatus.Cancelled,
            approvedByUserId: null,
            rejectionReason: e.Reason,
            updatedAt: e.UpdatedAt,
            ct
        );

    // =========================
    // Helpers
    // =========================

    private async Task UpsertCreatedAsync(
        Guid transferId,
        Guid fromBranchId,
        Guid toBranchId,
        string status,
        string notes,
        Guid createdByUserId,
        Guid? approvedByUserId,
        string? rejectionReason,
        IReadOnlyList<InventoryTransferLineSnapshot> lines,
        DateTime createdAt,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<InventoryTransferReadModel>(
            MongoCollections.InventoryTransfers
        );

        var id = $"transfer:{transferId}";
        var filter = Builders<InventoryTransferReadModel>.Filter.Eq(x => x.Id, id);

        var rmLines = lines
            .Select(l => new InventoryTransferLineReadModel
            {
                LineId = l.LineId,
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                FromWarehouseId = l.FromWarehouseId,
                ToWarehouseId = l.ToWarehouseId,
            })
            .ToList();

        var update = Builders<InventoryTransferReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.TransferId, transferId)
            .Set(x => x.FromBranchId, fromBranchId)
            .Set(x => x.ToBranchId, toBranchId)
            .Set(x => x.Status, status)
            .Set(x => x.Notes, notes)
            .Set(x => x.CreatedByUserId, createdByUserId)
            .Set(x => x.ApprovedByUserId, approvedByUserId)
            .Set(x => x.RejectionReason, rejectionReason)
            .Set(x => x.Lines, rmLines)
            .Set(x => x.UpdatedAt, updatedAt)
            .SetOnInsert(x => x.CreatedAt, createdAt);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task SetStatusAsync(
        Guid transferId,
        string status,
        Guid? approvedByUserId,
        string? rejectionReason,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<InventoryTransferReadModel>(
            MongoCollections.InventoryTransfers
        );
        var id = $"transfer:{transferId}";

        var update = Builders<InventoryTransferReadModel>
            .Update.Set(x => x.Status, status)
            .Set(x => x.ApprovedByUserId, approvedByUserId)
            .Set(x => x.RejectionReason, rejectionReason)
            .Set(x => x.UpdatedAt, updatedAt);

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }
}
