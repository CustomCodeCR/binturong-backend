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

namespace Application.Features.Inventory.Movements.RegisterPurchaseEntry;

internal sealed class RegisterPurchaseEntryCommandHandler
    : ICommandHandler<RegisterPurchaseEntryCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _realtime;
    private readonly IBackgroundJobScheduler _jobs;
    private readonly EmailOptions _emailOptions;

    private const string SourceModule = "Purchases";

    public RegisterPurchaseEntryCommandHandler(
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
        RegisterPurchaseEntryCommand command,
        CancellationToken ct
    )
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        if (command.Quantity <= 0)
        {
            await _bus.AuditAsync(
                userId,
                "Inventory",
                "InventoryMovement",
                null,
                "INVENTORY_PURCHASE_ENTRY_FAILED",
                string.Empty,
                $"reason=invalid_quantity; productId={command.ProductId}; warehouseId={command.WarehouseId}; qty={command.Quantity}; unitCost={command.UnitCost}; sourceId={command.SourceId}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(InventoryErrors.InvalidQuantity);
        }

        var warehouseExists = await _db.Warehouses.AnyAsync(x => x.Id == command.WarehouseId, ct);
        if (!warehouseExists)
        {
            await _bus.AuditAsync(
                userId,
                "Inventory",
                "InventoryMovement",
                null,
                "INVENTORY_PURCHASE_ENTRY_FAILED",
                string.Empty,
                $"reason=warehouse_not_found; productId={command.ProductId}; warehouseId={command.WarehouseId}; qty={command.Quantity}; unitCost={command.UnitCost}; sourceId={command.SourceId}",
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
        stock.CurrentStock += command.Quantity;

        var movement = new InventoryMovement
        {
            Id = Guid.NewGuid(),
            ProductId = command.ProductId,
            WarehouseFrom = null,
            WarehouseTo = command.WarehouseId,
            MovementType = InventoryMovementType.PurchaseIn,
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
            "INVENTORY_PURCHASE_ENTRY_REGISTERED",
            $"productId={command.ProductId}; warehouseId={command.WarehouseId}; beforeStock={beforeStock}",
            $"movementId={movement.Id}; movementType=PurchaseIn; qty={movement.Quantity}; unitCost={movement.UnitCost}; afterStock={stock.CurrentStock}; sourceModule={SourceModule}; sourceId={command.SourceId}; notes={movement.Notes}",
            ip,
            ua,
            ct
        );

        return Result.Success(movement.Id);
    }
}
