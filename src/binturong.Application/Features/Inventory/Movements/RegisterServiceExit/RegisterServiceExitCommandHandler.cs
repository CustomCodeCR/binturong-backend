using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Inventory;
using Domain.InventoryMovements;
using Domain.InventoryMovementTypes;
using Domain.WarehouseStocks;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Inventory.Movements.RegisterServiceExit;

internal sealed class RegisterServiceExitCommandHandler
    : ICommandHandler<RegisterServiceExitCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    private const string SourceModule = "ServiceOrders";

    public RegisterServiceExitCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(RegisterServiceExitCommand command, CancellationToken ct)
    {
        if (command.Quantity <= 0)
            return Result.Failure<Guid>(InventoryErrors.InvalidQuantity);

        var warehouseExists = await _db.Warehouses.AnyAsync(x => x.Id == command.WarehouseId, ct);
        if (!warehouseExists)
            return Result.Failure<Guid>(InventoryErrors.WarehouseNotFound(command.WarehouseId));

        var now = DateTime.UtcNow;

        var stock = await _db.WarehouseStocks.FirstOrDefaultAsync(
            x => x.ProductId == command.ProductId && x.WarehouseId == command.WarehouseId,
            ct
        );

        if (stock is null || stock.CurrentStock < command.Quantity)
            return Result.Failure<Guid>(
                Error.Failure("Inventory.StockInsufficient", "Insufficient stock")
            );

        stock.CurrentStock -= command.Quantity;

        var movement = new InventoryMovement
        {
            Id = Guid.NewGuid(),
            ProductId = command.ProductId,
            WarehouseFrom = command.WarehouseId,
            WarehouseTo = null,
            MovementType = InventoryMovementType.SaleOut,
            MovementDate = now,
            Quantity = command.Quantity,
            UnitCost = command.UnitCost,
            SourceModule = SourceModule,
            SourceId = command.SourceId,
            Notes = command.Notes ?? "",
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
