using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.InventoryTransfers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.InventoryTransfers.RequestReview;

internal sealed class RequestInventoryTransferReviewCommandHandler
    : ICommandHandler<RequestInventoryTransferReviewCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RequestInventoryTransferReviewCommandHandler(
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

    public async Task<Result> Handle(
        RequestInventoryTransferReviewCommand command,
        CancellationToken ct
    )
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var transfer = await _db
            .InventoryTransfers.Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == command.TransferId, ct);

        if (transfer is null)
        {
            await _bus.AuditAsync(
                userId,
                "InventoryTransfers",
                "InventoryTransfer",
                command.TransferId,
                "TRANSFER_REVIEW_REQUEST_FAILED",
                string.Empty,
                $"reason=not_found; transferId={command.TransferId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(InventoryTransferErrors.NotFound(command.TransferId));
        }

        if (transfer.Status != InventoryTransferStatus.Draft)
        {
            await _bus.AuditAsync(
                userId,
                "InventoryTransfers",
                "InventoryTransfer",
                transfer.Id,
                "TRANSFER_REVIEW_REQUEST_FAILED",
                $"status={transfer.Status}",
                "reason=invalid_status",
                ip,
                ua,
                ct
            );

            return Result.Failure(InventoryTransferErrors.InvalidStatus);
        }

        var before = $"status={transfer.Status}; lines={transfer.Lines.Count}";

        transfer.Status = InventoryTransferStatus.PendingReview;
        transfer.UpdatedAt = DateTime.UtcNow;

        transfer.RaiseReviewRequested();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "InventoryTransfers",
            "InventoryTransfer",
            transfer.Id,
            "TRANSFER_REVIEW_REQUESTED",
            before,
            $"status={transfer.Status}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
