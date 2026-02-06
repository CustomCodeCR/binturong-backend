using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.InventoryMovements;
using Domain.InventoryMovementTypes;
using Domain.InventoryTransfers;
using Domain.WarehouseStocks;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.InventoryTransfers.Confirm;

internal sealed class ConfirmInventoryTransferCommandHandler
    : ICommandHandler<ConfirmInventoryTransferCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    private const string SourceModule = "BranchTransfer";

    public ConfirmInventoryTransferCommandHandler(
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

    public async Task<Result> Handle(ConfirmInventoryTransferCommand command, CancellationToken ct)
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
                "TRANSFER_CONFIRM_FAILED",
                string.Empty,
                $"reason=not_found; transferId={command.TransferId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(InventoryTransferErrors.NotFound(command.TransferId));
        }

        var before =
            $"status={transfer.Status}; requireApproval={command.RequireApproval}; lines={transfer.Lines.Count}";

        // Approval flow
        if (command.RequireApproval)
        {
            if (transfer.Status == InventoryTransferStatus.Draft)
                return await Fail("requires_approval_draft");

            if (transfer.Status != InventoryTransferStatus.Approved)
                return await Fail("requires_approval_not_approved");
        }

        if (!command.RequireApproval)
        {
            if (
                transfer.Status != InventoryTransferStatus.Draft
                && transfer.Status != InventoryTransferStatus.Approved
            )
                return await Fail("invalid_status");
        }

        var now = DateTime.UtcNow;
        var movementCount = 0;

        foreach (var line in transfer.Lines)
        {
            var fromStock = await _db.WarehouseStocks.FirstOrDefaultAsync(
                x => x.ProductId == line.ProductId && x.WarehouseId == line.FromWarehouseId,
                ct
            );

            if (fromStock is null || fromStock.CurrentStock < line.Quantity)
            {
                await _bus.AuditAsync(
                    userId,
                    "InventoryTransfers",
                    "InventoryTransfer",
                    transfer.Id,
                    "TRANSFER_CONFIRM_FAILED",
                    before,
                    $"reason=stock_insufficient; productId={line.ProductId}; fromWarehouseId={line.FromWarehouseId}; qty={line.Quantity}; currentStock={fromStock?.CurrentStock}",
                    ip,
                    ua,
                    ct
                );

                return Result.Failure(
                    InventoryTransferErrors.StockInsufficient(line.ProductId, line.FromWarehouseId)
                );
            }

            var toStock = await _db.WarehouseStocks.FirstOrDefaultAsync(
                x => x.ProductId == line.ProductId && x.WarehouseId == line.ToWarehouseId,
                ct
            );

            if (toStock is null)
            {
                toStock = new WarehouseStock
                {
                    Id = Guid.NewGuid(),
                    ProductId = line.ProductId,
                    WarehouseId = line.ToWarehouseId,
                    CurrentStock = 0m,
                    MinStock = 0m,
                    MaxStock = 0m,
                };
                _db.WarehouseStocks.Add(toStock);
            }

            fromStock.CurrentStock -= line.Quantity;
            toStock.CurrentStock += line.Quantity;

            var movement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                ProductId = line.ProductId,
                WarehouseFrom = line.FromWarehouseId,
                WarehouseTo = line.ToWarehouseId,
                MovementType = InventoryMovementType.TransferIn,
                MovementDate = now,
                Quantity = line.Quantity,
                UnitCost = 0m,
                SourceModule = SourceModule,
                SourceId = null,
                Notes = $"Transfer {transfer.Id}",
            };

            movement.Raise(
                new InventoryMovementRegisteredDomainEvent(
                    movement.Id,
                    movement.ProductId,
                    movement.WarehouseFrom,
                    movement.WarehouseTo,
                    movement.MovementType,
                    movement.MovementDate,
                    movement.Quantity,
                    movement.UnitCost,
                    movement.SourceModule,
                    movement.SourceId,
                    string.IsNullOrWhiteSpace(movement.Notes) ? null : movement.Notes
                )
            );

            fromStock.Raise(
                new WarehouseStockChangedDomainEvent(
                    fromStock.Id,
                    fromStock.ProductId,
                    fromStock.WarehouseId,
                    fromStock.CurrentStock,
                    fromStock.MinStock,
                    fromStock.MaxStock,
                    now
                )
            );

            toStock.Raise(
                new WarehouseStockChangedDomainEvent(
                    toStock.Id,
                    toStock.ProductId,
                    toStock.WarehouseId,
                    toStock.CurrentStock,
                    toStock.MinStock,
                    toStock.MaxStock,
                    now
                )
            );

            _db.InventoryMovements.Add(movement);
            movementCount += 1;
        }

        transfer.Status = InventoryTransferStatus.Completed;
        transfer.UpdatedAt = now;
        transfer.RaiseConfirmed();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "InventoryTransfers",
            "InventoryTransfer",
            transfer.Id,
            "TRANSFER_CONFIRMED",
            before,
            $"status={transfer.Status}; movementCount={movementCount}; sourceModule={SourceModule}",
            ip,
            ua,
            ct
        );

        return Result.Success();

        async Task<Result> Fail(string reason)
        {
            await _bus.AuditAsync(
                userId,
                "InventoryTransfers",
                "InventoryTransfer",
                transfer.Id,
                "TRANSFER_CONFIRM_FAILED",
                before,
                $"reason={reason}; status={transfer.Status}",
                ip,
                ua,
                ct
            );

            return Result.Failure(InventoryTransferErrors.RequiresApproval);
        }
    }
}
