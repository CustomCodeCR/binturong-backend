using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.InventoryTransfers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.InventoryTransfers.RequestReview;

internal sealed class RequestInventoryTransferReviewCommandHandler
    : ICommandHandler<RequestInventoryTransferReviewCommand>
{
    private readonly IApplicationDbContext _db;

    public RequestInventoryTransferReviewCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(
        RequestInventoryTransferReviewCommand command,
        CancellationToken ct
    )
    {
        var transfer = await _db
            .InventoryTransfers.Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == command.TransferId, ct);

        if (transfer is null)
            return Result.Failure(InventoryTransferErrors.NotFound(command.TransferId));

        if (transfer.Status != InventoryTransferStatus.Draft)
            return Result.Failure(InventoryTransferErrors.InvalidStatus);

        transfer.Status = InventoryTransferStatus.PendingReview;
        transfer.UpdatedAt = DateTime.UtcNow;

        transfer.RaiseReviewRequested();

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
