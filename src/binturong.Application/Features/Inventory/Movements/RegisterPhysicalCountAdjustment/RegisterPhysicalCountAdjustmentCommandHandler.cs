using Application.Abstractions.Background;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.Features.Inventory.Alerts;
using Application.Options;
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
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _realtime;
    private readonly IBackgroundJobScheduler _jobs;
    private readonly EmailOptions _emailOptions;

    private const string SourceModule = "InventoryAudit";

    public RegisterPhysicalCountAdjustmentCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser,
        IRealtimeNotifier realtime,
        IBackgroundJobScheduler jobs,
        EmailOptions emailOptions
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
        _realtime = realtime;
        _jobs = jobs;
        _emailOptions = emailOptions;
    }

    public async Task<Result<Guid>> Handle(
        RegisterPhysicalCountAdjustmentCommand command,
        CancellationToken ct
    )
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        if (command.CountedStock < 0)
        {
            await _bus.AuditAsync(
                userId,
                "Inventory",
                "InventoryMovement",
                null,
                "INVENTORY_PHYSICAL_COUNT_ADJUSTMENT_FAILED",
                string.Empty,
                $"reason=invalid_counted_stock; productId={command.ProductId}; warehouseId={command.WarehouseId}; countedStock={command.CountedStock}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(InventoryErrors.InvalidCountedStock);
        }

        var warehouseExists = await _db.Warehouses.AnyAsync(x => x.Id == command.WarehouseId, ct);
        if (!warehouseExists)
        {
            await _bus.AuditAsync(
                userId,
                "Inventory",
                "InventoryMovement",
                null,
                "INVENTORY_PHYSICAL_COUNT_ADJUSTMENT_FAILED",
                string.Empty,
                $"reason=warehouse_not_found; productId={command.ProductId}; warehouseId={command.WarehouseId}; countedStock={command.CountedStock}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(InventoryErrors.WarehouseNotFound(command.WarehouseId));
        }

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
                LowStockAlertActive = false,
                LowStockLastNotifiedAtUtc = null,
            };
            _db.WarehouseStocks.Add(stock);
        }

        var beforeStock = stock.CurrentStock;

        var diff = command.CountedStock - stock.CurrentStock;
        if (diff == 0m)
        {
            await _bus.AuditAsync(
                userId,
                "Inventory",
                "InventoryMovement",
                null,
                "INVENTORY_PHYSICAL_COUNT_ADJUSTMENT_NOOP",
                string.Empty,
                $"productId={command.ProductId}; warehouseId={command.WarehouseId}; countedStock={command.CountedStock}; beforeStock={beforeStock}; diff=0",
                ip,
                ua,
                ct
            );

            return Result.Success(Guid.Empty);
        }

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

        await LowStockAlertHelper.HandleLowStockAsync(
            stock,
            stock.ProductId,
            stock.WarehouseId,
            now,
            _realtime,
            _jobs,
            _emailOptions,
            _currentUser,
            ct
        );

        _db.InventoryMovements.Add(movement);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Inventory",
            "InventoryMovement",
            movement.Id,
            "INVENTORY_PHYSICAL_COUNT_ADJUSTMENT_REGISTERED",
            $"productId={command.ProductId}; warehouseId={command.WarehouseId}; beforeStock={beforeStock}",
            $"movementId={movement.Id}; movementType={movementType}; qty={movement.Quantity}; unitCost={movement.UnitCost}; countedStock={command.CountedStock}; diff={diff}; sourceModule={SourceModule}; notes={movement.Notes}",
            ip,
            ua,
            ct
        );

        return Result.Success(movement.Id);
    }
}
