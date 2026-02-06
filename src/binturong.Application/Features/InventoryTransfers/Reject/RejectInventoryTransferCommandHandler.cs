using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.InventoryTransfers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.InventoryTransfers.Reject;

internal sealed class RejectInventoryTransferCommandHandler
    : ICommandHandler<RejectInventoryTransferCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;

    public RejectInventoryTransferCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
    }

    public async Task<Result> Handle(RejectInventoryTransferCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;

        var transfer = await _db.InventoryTransfers.FirstOrDefaultAsync(
            x => x.Id == command.TransferId,
            ct
        );
        if (transfer is null)
        {
            await _bus.AuditAsync(
                command.RejectedByUserId,
                "InventoryTransfers",
                "InventoryTransfer",
                command.TransferId,
                "TRANSFER_REJECT_FAILED",
                string.Empty,
                $"reason=not_found; transferId={command.TransferId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(InventoryTransferErrors.NotFound(command.TransferId));
        }

        if (transfer.Status != InventoryTransferStatus.PendingReview)
        {
            await _bus.AuditAsync(
                command.RejectedByUserId,
                "InventoryTransfers",
                "InventoryTransfer",
                transfer.Id,
                "TRANSFER_REJECT_FAILED",
                $"status={transfer.Status}",
                "reason=invalid_status",
                ip,
                ua,
                ct
            );

            return Result.Failure(InventoryTransferErrors.InvalidStatus);
        }

        var before = $"status={transfer.Status}";
        transfer.Status = InventoryTransferStatus.Rejected;
        transfer.UpdatedAt = DateTime.UtcNow;

        transfer.RaiseRejected(command.RejectedByUserId, command.Reason);

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            command.RejectedByUserId,
            "InventoryTransfers",
            "InventoryTransfer",
            transfer.Id,
            "TRANSFER_REJECTED",
            before,
            $"status={transfer.Status}; reason={command.Reason}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
