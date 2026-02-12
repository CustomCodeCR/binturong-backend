using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.SalesOrders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.SalesOrders.Create;

internal sealed class CreateSalesOrderCommandHandler
    : ICommandHandler<CreateSalesOrderCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateSalesOrderCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateSalesOrderCommand cmd, CancellationToken ct)
    {
        if (cmd.ClientId == Guid.Empty)
            return Result.Failure<Guid>(SalesOrderErrors.ClientRequired);

        if (cmd.Lines is null || cmd.Lines.Count == 0)
            return Result.Failure<Guid>(SalesOrderErrors.DetailsRequired);

        if (!await _db.Clients.AnyAsync(x => x.Id == cmd.ClientId, ct))
            return Result.Failure<Guid>(
                Error.NotFound("Clients.NotFound", $"Client '{cmd.ClientId}' not found.")
            );

        if (cmd.BranchId is not null && !await _db.Branches.AnyAsync(x => x.Id == cmd.BranchId, ct))
            return Result.Failure<Guid>(
                Error.NotFound("Branches.NotFound", $"Branch '{cmd.BranchId}' not found.")
            );

        if (
            cmd.SellerUserId is not null
            && !await _db.Users.AnyAsync(x => x.Id == cmd.SellerUserId, ct)
        )
            return Result.Failure<Guid>(
                Error.NotFound("Users.NotFound", $"User '{cmd.SellerUserId}' not found.")
            );

        var now = DateTime.UtcNow;

        var so = new SalesOrder
        {
            Id = Guid.NewGuid(),
            Code = await NextCodeAsync(ct),
            ClientId = cmd.ClientId,
            BranchId = cmd.BranchId,
            SellerUserId = cmd.SellerUserId,
            OrderDate = now,
            Status = "Draft",
            Currency = cmd.Currency.Trim(),
            ExchangeRate = cmd.ExchangeRate,
            Notes = cmd.Notes?.Trim() ?? string.Empty,
            CreatedAt = now,
            UpdatedAt = now,
        };

        decimal subtotal = 0m,
            taxes = 0m,
            discounts = 0m,
            total = 0m;

        foreach (var l in cmd.Lines)
        {
            if (l.ProductId == Guid.Empty)
                return Result.Failure<Guid>(
                    Error.Validation("SalesOrders.ProductRequired", "ProductId is required.")
                );

            if (l.Quantity <= 0)
                return Result.Failure<Guid>(
                    Error.Validation("SalesOrders.InvalidQuantity", "Quantity must be > 0.")
                );

            if (l.UnitPrice <= 0)
                return Result.Failure<Guid>(
                    Error.Validation("SalesOrders.InvalidUnitPrice", "UnitPrice must be > 0.")
                );

            var lineBase = l.Quantity * l.UnitPrice;
            var lineDiscount = lineBase * (l.DiscountPerc / 100m);
            var lineTax = (lineBase - lineDiscount) * (l.TaxPerc / 100m);
            var lineTotal = (lineBase - lineDiscount) + lineTax;

            var d = new Domain.SalesOrderDetails.SalesOrderDetail
            {
                Id = Guid.NewGuid(),
                SalesOrderId = so.Id,
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                DiscountPerc = l.DiscountPerc,
                TaxPerc = l.TaxPerc,
                LineTotal = lineTotal,
            };

            so.Details.Add(d);
            so.RaiseDetailAdded(d, now);

            subtotal += lineBase;
            discounts += lineDiscount;
            taxes += lineTax;
            total += lineTotal;
        }

        so.Subtotal = subtotal;
        so.Discounts = discounts;
        so.Taxes = taxes;
        so.Total = total;

        so.RaiseCreated();

        _db.SalesOrders.Add(so);
        await _db.SaveChangesAsync(ct);

        return so.Id;
    }

    private async Task<string> NextCodeAsync(CancellationToken ct)
    {
        var count = await _db.SalesOrders.CountAsync(ct);
        return $"SO-{(count + 1):000000}";
    }
}
