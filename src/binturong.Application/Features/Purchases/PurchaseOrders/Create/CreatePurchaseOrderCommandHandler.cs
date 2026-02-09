using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.PurchaseOrderDetails;
using Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Purchases.PurchaseOrders.Create;

internal sealed class CreatePurchaseOrderCommandHandler
    : ICommandHandler<CreatePurchaseOrderCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreatePurchaseOrderCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreatePurchaseOrderCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var code = cmd.Code.Trim();
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Guid>(PurchaseOrderErrors.CodeRequired);

        if (cmd.SupplierId == Guid.Empty)
            return Result.Failure<Guid>(PurchaseOrderErrors.SupplierRequired);

        if (cmd.Lines is null || cmd.Lines.Count == 0)
            return Result.Failure<Guid>(PurchaseOrderErrors.NoLines);

        if (cmd.Lines.Any(l => l.ProductId == Guid.Empty))
            return Result.Failure<Guid>(PurchaseOrderErrors.ProductRequired);

        if (cmd.Lines.Any(l => l.Quantity <= 0))
            return Result.Failure<Guid>(PurchaseOrderErrors.QuantityRequired);

        if (cmd.Lines.Any(l => l.UnitPrice <= 0))
            return Result.Failure<Guid>(PurchaseOrderErrors.UnitPriceInvalid);

        if (string.IsNullOrWhiteSpace(cmd.Currency))
            return Result.Failure<Guid>(PurchaseOrderErrors.CurrencyRequired);

        if (cmd.ExchangeRate <= 0)
            return Result.Failure<Guid>(PurchaseOrderErrors.ExchangeRateInvalid);

        var codeExists = await _db.PurchaseOrders.AnyAsync(
            x => x.Code.ToLower() == code.ToLower(),
            ct
        );
        if (codeExists)
            return Result.Failure<Guid>(PurchaseOrderErrors.CodeNotUnique);

        var supplierExists = await _db.Suppliers.AnyAsync(x => x.Id == cmd.SupplierId, ct);
        if (!supplierExists)
            return Result.Failure<Guid>(
                Error.NotFound("Suppliers.NotFound", $"Supplier '{cmd.SupplierId}' not found")
            );

        // Totals
        decimal subtotal = 0m;
        decimal taxes = 0m;
        decimal discounts = 0m;

        foreach (var l in cmd.Lines)
        {
            var lineBase = l.Quantity * l.UnitPrice;

            var lineDiscount = lineBase * (l.DiscountPerc / 100m);
            var discounted = lineBase - lineDiscount;

            var lineTax = discounted * (l.TaxPerc / 100m);

            subtotal += lineBase;
            discounts += lineDiscount;
            taxes += lineTax;
        }

        var total = subtotal - discounts + taxes;

        var po = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            Code = code,
            SupplierId = cmd.SupplierId,
            BranchId = cmd.BranchId,
            RequestId = cmd.RequestId,
            OrderDate = cmd.OrderDateUtc,
            Status = "Pending",
            Currency = cmd.Currency.Trim(),
            ExchangeRate = cmd.ExchangeRate,
            Subtotal = subtotal,
            Taxes = taxes,
            Discounts = discounts,
            Total = total,
        };

        // Add details via aggregate method (raises PurchaseOrderLineAddedDomainEvent)
        foreach (var l in cmd.Lines)
        {
            var lineBase = l.Quantity * l.UnitPrice;
            var lineDiscount = lineBase * (l.DiscountPerc / 100m);
            var discounted = lineBase - lineDiscount;
            var lineTax = discounted * (l.TaxPerc / 100m);
            var lineTotal = discounted + lineTax;

            var detail = new PurchaseOrderDetail
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = po.Id,
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                DiscountPerc = l.DiscountPerc,
                TaxPerc = l.TaxPerc,
                LineTotal = lineTotal,
            };

            po.AddDetail(detail);
        }

        // Raise aggregate created event (PurchaseOrderCreatedDomainEvent)
        po.RaiseCreated();

        _db.PurchaseOrders.Add(po);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Purchases",
            "PurchaseOrder",
            po.Id,
            "PURCHASE_ORDER_CREATED",
            string.Empty,
            $"purchaseOrderId={po.Id}; code={po.Code}; supplierId={po.SupplierId}; lines={po.Details.Count}; status={po.Status}; currency={po.Currency}; total={po.Total}",
            ip,
            ua,
            ct
        );

        return Result.Success(po.Id);
    }
}
