using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.PurchaseReceiptDetails;
using Domain.PurchaseReceipts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Purchases.PurchaseReceipts.Create;

internal sealed class CreatePurchaseReceiptCommandHandler
    : ICommandHandler<CreatePurchaseReceiptCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreatePurchaseReceiptCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreatePurchaseReceiptCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        if (cmd.PurchaseOrderId == Guid.Empty)
            return Result.Failure<Guid>(PurchaseReceiptErrors.PurchaseOrderRequired);

        if (cmd.WarehouseId == Guid.Empty)
            return Result.Failure<Guid>(PurchaseReceiptErrors.WarehouseRequired);

        if (cmd.Lines is null || cmd.Lines.Count == 0)
            return Result.Failure<Guid>(PurchaseReceiptErrors.NoLines);

        if (cmd.Lines.Any(l => l.ProductId == Guid.Empty))
            return Result.Failure<Guid>(PurchaseReceiptErrors.ProductRequired);

        if (cmd.Lines.Any(l => l.QuantityReceived <= 0))
            return Result.Failure<Guid>(PurchaseReceiptErrors.QuantityInvalid);

        if (cmd.Lines.Any(l => l.UnitCost <= 0))
            return Result.Failure<Guid>(PurchaseReceiptErrors.UnitCostInvalid);

        var po = await _db
            .PurchaseOrders.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == cmd.PurchaseOrderId, ct);

        if (po is null)
            return Result.Failure<Guid>(
                Error.NotFound(
                    "PurchaseOrders.NotFound",
                    $"Purchase order '{cmd.PurchaseOrderId}' not found"
                )
            );

        var whExists = await _db.Warehouses.AnyAsync(x => x.Id == cmd.WarehouseId, ct);
        if (!whExists)
            return Result.Failure<Guid>(
                Error.NotFound("Warehouses.NotFound", $"Warehouse '{cmd.WarehouseId}' not found")
            );

        var orderedByProduct = po
            .Details.GroupBy(d => d.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        foreach (var l in cmd.Lines)
        {
            if (!orderedByProduct.TryGetValue(l.ProductId, out var orderedQty))
                return Result.Failure<Guid>(
                    PurchaseReceiptErrors.ProductNotInOrder(l.ProductId, po.Id)
                );

            if (l.QuantityReceived > orderedQty)
                return Result.Failure<Guid>(
                    PurchaseReceiptErrors.QuantityExceedsOrdered(
                        l.ProductId,
                        l.QuantityReceived,
                        orderedQty
                    )
                );
        }

        var isPartial = cmd.Lines.Any(l => l.QuantityReceived < orderedByProduct[l.ProductId]);
        var receiptStatus = isPartial ? "PartiallyReceived" : "Completed";

        var receipt = new PurchaseReceipt
        {
            Id = Guid.NewGuid(),
            PurchaseOrderId = po.Id,
            WarehouseId = cmd.WarehouseId,
            ReceiptDate = cmd.ReceiptDateUtc,
            Status = receiptStatus,
            Notes = cmd.Notes?.Trim() ?? string.Empty,
        };

        receipt.RaiseCreated();

        foreach (var l in cmd.Lines)
        {
            var detail = new PurchaseReceiptDetail
            {
                Id = Guid.NewGuid(),
                ReceiptId = receipt.Id,
                ProductId = l.ProductId,
                QuantityReceived = l.QuantityReceived,
                UnitCost = l.UnitCost,
            };

            receipt.AddDetail(detail);
        }

        _db.PurchaseReceipts.Add(receipt);

        if (isPartial)
            po.MarkPartiallyReceived();
        else
            po.MarkCompleted();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Purchases",
            "PurchaseReceipt",
            receipt.Id,
            "PURCHASE_RECEIPT_CREATED",
            string.Empty,
            $"receiptId={receipt.Id}; purchaseOrderId={po.Id}; warehouseId={receipt.WarehouseId}; lines={receipt.Details.Count}; receiptStatus={receipt.Status}; orderStatus={po.Status}",
            ip,
            ua,
            ct
        );

        return Result.Success(receipt.Id);
    }
}
