using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.SalesOrders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.SalesOrders.ConvertFromQuote;

internal sealed class ConvertQuoteToSalesOrderCommandHandler
    : ICommandHandler<ConvertQuoteToSalesOrderCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public ConvertQuoteToSalesOrderCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(
        ConvertQuoteToSalesOrderCommand cmd,
        CancellationToken ct
    )
    {
        var quote = await _db
            .Quotes.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == cmd.QuoteId, ct);

        if (quote is null)
            return Result.Failure<Guid>(SalesOrderErrors.QuoteNotFound(cmd.QuoteId));

        // HU-VTA-01:
        // "Approved" in your domain == Quote.Accept() => Status = "Accepted"
        if (!string.Equals(quote.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
            return Result.Failure<Guid>(SalesOrderErrors.QuoteNotAccepted(cmd.QuoteId));

        // Quote.ValidUntil is NOT nullable in your domain model
        if (DateTime.UtcNow > quote.ValidUntil)
            return Result.Failure<Guid>(
                SalesOrderErrors.QuoteExpired(cmd.QuoteId, quote.ValidUntil)
            );

        // Optional: prevent converting the same quote multiple times
        var alreadyConverted = await _db.SalesOrders.AnyAsync(x => x.QuoteId == quote.Id, ct);
        if (alreadyConverted)
            return Result.Failure<Guid>(SalesOrderErrors.QuoteAlreadyConverted(quote.Id));

        var now = DateTime.UtcNow;

        var so = new SalesOrder
        {
            Id = Guid.NewGuid(),
            Code = await NextCodeAsync(ct),
            QuoteId = quote.Id,
            ClientId = quote.ClientId,
            BranchId = cmd.BranchId ?? quote.BranchId,
            OrderDate = now,
            Status = "Draft",
            Currency = cmd.Currency.Trim(),
            ExchangeRate = cmd.ExchangeRate,
            Notes = cmd.Notes?.Trim() ?? string.Empty,
        };

        decimal subtotal = 0m,
            taxes = 0m,
            discounts = 0m,
            total = 0m;

        foreach (var ql in quote.Details)
        {
            var lineBase = ql.Quantity * ql.UnitPrice;
            var lineDiscount = lineBase * (ql.DiscountPerc / 100m);
            var lineTax = (lineBase - lineDiscount) * (ql.TaxPerc / 100m);
            var lineTotal = (lineBase - lineDiscount) + lineTax;

            var d = new Domain.SalesOrderDetails.SalesOrderDetail
            {
                Id = Guid.NewGuid(),
                SalesOrderId = so.Id,
                ProductId = ql.ProductId,
                Quantity = ql.Quantity,
                UnitPrice = ql.UnitPrice,
                DiscountPerc = ql.DiscountPerc,
                TaxPerc = ql.TaxPerc,
                LineTotal = lineTotal,
            };

            so.Details.Add(d);

            // Your SalesOrder events (keep as you designed)
            so.RaiseDetailAdded(d);

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
        so.RaiseConvertedFromQuote(quote.Id);

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
