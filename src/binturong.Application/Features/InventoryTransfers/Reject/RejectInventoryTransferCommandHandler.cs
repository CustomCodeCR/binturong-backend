using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.InventoryTransfers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.InventoryTransfers.Reject;

internal sealed class RejectInventoryTransferCommandHandler
    : ICommandHandler<RejectInventoryTransferCommand>
{
    private readonly IApplicationDbContext _db;

    public RejectInventoryTransferCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(RejectInventoryTransferCommand command, CancellationToken ct)
    {
        var transfer = await _db.InventoryTransfers.FirstOrDefaultAsync(
            x => x.Id == command.TransferId,
            ct
        );
        if (transfer is null)
            return Result.Failure(InventoryTransferErrors.NotFound(command.TransferId));

        if (transfer.Status != InventoryTransferStatus.PendingReview)
            return Result.Failure(InventoryTransferErrors.InvalidStatus);

        transfer.Status = InventoryTransferStatus.Rejected;
        transfer.UpdatedAt = DateTime.UtcNow;

        transfer.RaiseRejected(command.RejectedByUserId, command.Reason);

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
