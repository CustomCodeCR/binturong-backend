using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Inventory;
using Domain.InventoryMovements;
using Domain.InventoryMovementTypes;
using Domain.WarehouseStocks;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Inventory.Movements.RegisterPhysicalCountAdjustment;

internal sealed class RegisterPhysicalCountAdjustmentCommandHandler
    : ICommandHandler<RegisterPhysicalCountAdjustmentCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    private const string SourceModule = "InventoryAudit";

    public RegisterPhysicalCountAdjustmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(
        RegisterPhysicalCountAdjustmentCommand command,
        CancellationToken ct
    )
    {
        if (command.CountedStock < 0)
            return Result.Failure<Guid>(InventoryErrors.InvalidCountedStock);

        var warehouseExists = await _db.Warehouses.AnyAsync(x => x.Id == command.WarehouseId, ct);
        if (!warehouseExists)
            return Result.Failure<Guid>(InventoryErrors.WarehouseNotFound(command.WarehouseId));

        var now = DateTime.UtcNow;

        var stock = await _db.WarehouseStocks.FirstOrDefaultAsync(
            x => x.ProductId == command.ProductId && x.WarehouseId == command.WarehouseId,
            ct
        );

        if (stock is null)
        {
            stock = new WarehouseStock
            {
                Id = Guid.NewGuid(),
                ProductId = command.ProductId,
                WarehouseId = command.WarehouseId,
                CurrentStock = 0m,
                MinStock = 0m,
                MaxStock = 0m,
            };
            _db.WarehouseStocks.Add(stock);
        }

        var diff = command.CountedStock - stock.CurrentStock;
        if (diff == 0m)
            return Result.Success(Guid.Empty); // no movement needed (optional behavior)

        // apply stock
        stock.CurrentStock = command.CountedStock;

        var movementType =
            diff > 0 ? InventoryMovementType.AdjustmentIn : InventoryMovementType.AdjustmentOut;

        var movement = new InventoryMovement
        {
            Id = Guid.NewGuid(),
            ProductId = command.ProductId,
            WarehouseFrom = diff < 0 ? command.WarehouseId : null,
            WarehouseTo = diff > 0 ? command.WarehouseId : null,
            MovementType = movementType,
            MovementDate = now,
            Quantity = Math.Abs(diff),
            UnitCost = command.UnitCost,
            SourceModule = SourceModule,
            SourceId = null,
            Notes = $"Physical count adjustment: {command.Justification}",
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

        stock.Raise(
            new WarehouseStockChangedDomainEvent(
                stock.Id,
                stock.ProductId,
                stock.WarehouseId,
                stock.CurrentStock,
                stock.MinStock,
                stock.MaxStock,
                now
            )
        );

        _db.InventoryMovements.Add(movement);
        await _db.SaveChangesAsync(ct);

        return Result.Success(movement.Id);
    }
}
