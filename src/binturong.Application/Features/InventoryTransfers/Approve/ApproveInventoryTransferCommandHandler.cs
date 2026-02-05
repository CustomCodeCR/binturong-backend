using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.InventoryTransfers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.InventoryTransfers.Approve;

internal sealed class ApproveInventoryTransferCommandHandler
    : ICommandHandler<ApproveInventoryTransferCommand>
{
    private readonly IApplicationDbContext _db;

    public ApproveInventoryTransferCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(ApproveInventoryTransferCommand command, CancellationToken ct)
    {
        var transfer = await _db.InventoryTransfers.FirstOrDefaultAsync(
            x => x.Id == command.TransferId,
            ct
        );
        if (transfer is null)
            return Result.Failure(InventoryTransferErrors.NotFound(command.TransferId));

        if (transfer.Status != InventoryTransferStatus.PendingReview)
            return Result.Failure(InventoryTransferErrors.InvalidStatus);

        transfer.Status = InventoryTransferStatus.Approved;
        transfer.ApprovedByUserId = command.ApprovedByUserId;
        transfer.UpdatedAt = DateTime.UtcNow;

        transfer.RaiseApproved(command.ApprovedByUserId);

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
