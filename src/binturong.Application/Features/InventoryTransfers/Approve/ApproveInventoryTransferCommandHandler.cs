using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.InventoryTransfers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.InventoryTransfers.Approve;

internal sealed class ApproveInventoryTransferCommandHandler
    : ICommandHandler<ApproveInventoryTransferCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;

    public ApproveInventoryTransferCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
    }

    public async Task<Result> Handle(ApproveInventoryTransferCommand command, CancellationToken ct)
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
                command.ApprovedByUserId,
                "InventoryTransfers",
                "InventoryTransfer",
                command.TransferId,
                "TRANSFER_APPROVE_FAILED",
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
                command.ApprovedByUserId,
                "InventoryTransfers",
                "InventoryTransfer",
                transfer.Id,
                "TRANSFER_APPROVE_FAILED",
                $"status={transfer.Status}",
                "reason=invalid_status",
                ip,
                ua,
                ct
            );

            return Result.Failure(InventoryTransferErrors.InvalidStatus);
        }

        var before = $"status={transfer.Status}; approvedByUserId={transfer.ApprovedByUserId}";

        transfer.Status = InventoryTransferStatus.Approved;
        transfer.ApprovedByUserId = command.ApprovedByUserId;
        transfer.UpdatedAt = DateTime.UtcNow;

        transfer.RaiseApproved(command.ApprovedByUserId);

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            command.ApprovedByUserId,
            "InventoryTransfers",
            "InventoryTransfer",
            transfer.Id,
            "TRANSFER_APPROVED",
            before,
            $"status={transfer.Status}; approvedByUserId={transfer.ApprovedByUserId}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
