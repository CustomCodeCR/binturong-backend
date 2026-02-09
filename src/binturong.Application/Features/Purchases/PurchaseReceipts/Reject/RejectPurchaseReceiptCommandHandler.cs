using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Purchases.PurchaseReceipts.Reject;

internal sealed class RejectPurchaseReceiptCommandHandler
    : ICommandHandler<RejectPurchaseReceiptCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RejectPurchaseReceiptCommandHandler(
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

    public async Task<Result> Handle(RejectPurchaseReceiptCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        if (cmd.ReceiptId == Guid.Empty)
            return Result.Failure(
                Error.Validation("PurchaseReceipts.IdRequired", "ReceiptId is required")
            );

        if (string.IsNullOrWhiteSpace(cmd.Reason))
            return Result.Failure(
                Error.Validation("PurchaseReceipts.RejectReasonRequired", "Reason is required")
            );

        var receipt = await _db.PurchaseReceipts.FirstOrDefaultAsync(
            x => x.Id == cmd.ReceiptId,
            ct
        );
        if (receipt is null)
            return Result.Failure(
                Error.NotFound(
                    "PurchaseReceipts.NotFound",
                    $"Purchase receipt '{cmd.ReceiptId}' not found"
                )
            );

        // HU-COM-02 scenario 3: do NOT update inventory here (that should be handled by your inventory flow)
        receipt.Status = "Rejected";

        var reason = cmd.Reason.Trim();
        receipt.Notes = string.IsNullOrWhiteSpace(receipt.Notes)
            ? $"Rejected: {reason}"
            : $"{receipt.Notes}\nRejected: {reason}";

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Purchases",
            "PurchaseReceipt",
            receipt.Id,
            "PURCHASE_RECEIPT_REJECTED",
            string.Empty,
            $"receiptId={receipt.Id}; rejectedAt={cmd.RejectedAtUtc:o}; reason={reason}; status={receipt.Status}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
