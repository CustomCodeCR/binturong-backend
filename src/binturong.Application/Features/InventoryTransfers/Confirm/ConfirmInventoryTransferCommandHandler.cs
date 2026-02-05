using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
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

    private const string SourceModule = "BranchTransfer";

    public ConfirmInventoryTransferCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(ConfirmInventoryTransferCommand command, CancellationToken ct)
    {
        var transfer = await _db
            .InventoryTransfers.Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == command.TransferId, ct);

        if (transfer is null)
            return Result.Failure(InventoryTransferErrors.NotFound(command.TransferId));

        // Approval flow
        if (command.RequireApproval)
        {
            if (transfer.Status == InventoryTransferStatus.Draft)
                return Result.Failure(InventoryTransferErrors.RequiresApproval);

            if (transfer.Status != InventoryTransferStatus.Approved)
                return Result.Failure(InventoryTransferErrors.RequiresApproval);
        }

        // If not requiring approval: allow Draft or Approved (your call)
        if (!command.RequireApproval)
        {
            if (
                transfer.Status != InventoryTransferStatus.Draft
                && transfer.Status != InventoryTransferStatus.Approved
            )
                return Result.Failure(InventoryTransferErrors.InvalidStatus);
        }

        var now = DateTime.UtcNow;

        foreach (var line in transfer.Lines)
        {
            // FROM stock
            var fromStock = await _db.WarehouseStocks.FirstOrDefaultAsync(
                x => x.ProductId == line.ProductId && x.WarehouseId == line.FromWarehouseId,
                ct
            );

            if (fromStock is null || fromStock.CurrentStock < line.Quantity)
                return Result.Failure(
                    InventoryTransferErrors.StockInsufficient(line.ProductId, line.FromWarehouseId)
                );

            // TO stock upsert
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
        }

        transfer.Status = InventoryTransferStatus.Completed;
        transfer.UpdatedAt = now;
        transfer.RaiseConfirmed();

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
