using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.InventoryTransfers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.InventoryTransfers.Delete;

internal sealed class DeleteInventoryTransferCommandHandler
    : ICommandHandler<DeleteInventoryTransferCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteInventoryTransferCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteInventoryTransferCommand command, CancellationToken ct)
    {
        var transfer = await _db.InventoryTransfers.FirstOrDefaultAsync(
            x => x.Id == command.TransferId,
            ct
        );
        if (transfer is null)
            return Result.Failure(InventoryTransferErrors.NotFound(command.TransferId));

        // Usually only allow delete if Draft or Rejected
        if (
            transfer.Status != InventoryTransferStatus.Draft
            && transfer.Status != InventoryTransferStatus.Rejected
        )
            return Result.Failure(InventoryTransferErrors.InvalidStatus);

        transfer.RaiseDeleted();

        _db.InventoryTransfers.Remove(transfer);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
