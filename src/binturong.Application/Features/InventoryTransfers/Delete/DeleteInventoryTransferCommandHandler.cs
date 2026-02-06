using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.InventoryTransfers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.InventoryTransfers.Delete;

internal sealed class DeleteInventoryTransferCommandHandler
    : ICommandHandler<DeleteInventoryTransferCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteInventoryTransferCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteInventoryTransferCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var transfer = await _db.InventoryTransfers.FirstOrDefaultAsync(
            x => x.Id == command.TransferId,
            ct
        );

        if (transfer is null)
        {
            await _bus.AuditAsync(
                userId,
                "InventoryTransfers",
                "InventoryTransfer",
                command.TransferId,
                "TRANSFER_DELETE_FAILED",
                string.Empty,
                $"reason=not_found; transferId={command.TransferId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(InventoryTransferErrors.NotFound(command.TransferId));
        }

        if (
            transfer.Status != InventoryTransferStatus.Draft
            && transfer.Status != InventoryTransferStatus.Rejected
        )
        {
            await _bus.AuditAsync(
                userId,
                "InventoryTransfers",
                "InventoryTransfer",
                transfer.Id,
                "TRANSFER_DELETE_FAILED",
                $"status={transfer.Status}",
                "reason=invalid_status",
                ip,
                ua,
                ct
            );

            return Result.Failure(InventoryTransferErrors.InvalidStatus);
        }

        var before = $"status={transfer.Status}; notes={transfer.Notes}";

        transfer.RaiseDeleted();

        _db.InventoryTransfers.Remove(transfer);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "InventoryTransfers",
            "InventoryTransfer",
            transfer.Id,
            "TRANSFER_DELETED",
            before,
            $"transferId={transfer.Id}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
