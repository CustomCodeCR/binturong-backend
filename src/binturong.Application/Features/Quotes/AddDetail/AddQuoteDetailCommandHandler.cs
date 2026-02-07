using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.QuoteDetails;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Quotes.AddDetail;

internal sealed class AddQuoteDetailCommandHandler : ICommandHandler<AddQuoteDetailCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public AddQuoteDetailCommandHandler(
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

    public async Task<Result<Guid>> Handle(AddQuoteDetailCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        if (command.QuoteId == Guid.Empty)
            return Result.Failure<Guid>(
                Error.Validation("Quotes.QuoteRequired", "QuoteId is required")
            );
        if (command.ProductId == Guid.Empty)
            return Result.Failure<Guid>(
                Error.Validation("Quotes.ProductRequired", "ProductId is required")
            );
        if (command.Quantity <= 0)
            return Result.Failure<Guid>(
                Error.Validation("Quotes.QuantityInvalid", "Quantity must be > 0")
            );
        if (command.UnitPrice <= 0)
            return Result.Failure<Guid>(
                Error.Validation("Quotes.UnitPriceInvalid", "UnitPrice must be > 0")
            );

        var quote = await _db
            .Quotes.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == command.QuoteId, ct);

        if (quote is null)
            return Result.Failure<Guid>(
                Error.NotFound("Quotes.NotFound", $"Quote '{command.QuoteId}' not found")
            );

        // Cálculos
        var discountFactor = 1m - (command.DiscountPerc / 100m);
        if (discountFactor < 0)
            discountFactor = 0;

        var lineBase = command.Quantity * command.UnitPrice;
        var lineAfterDiscount = lineBase * discountFactor;
        var taxes = lineAfterDiscount * (command.TaxPerc / 100m);
        var lineTotal = lineAfterDiscount + taxes;

        var detail = new QuoteDetail
        {
            Id = Guid.NewGuid(),
            QuoteId = quote.Id,
            ProductId = command.ProductId,
            Quantity = command.Quantity,
            UnitPrice = command.UnitPrice,
            DiscountPerc = command.DiscountPerc,
            TaxPerc = command.TaxPerc,
            LineTotal = lineTotal,
        };

        // Esto dispara QuoteDetailAddedDomainEvent
        quote.AddDetail(detail);

        // Totales del quote (simple: recalcular sumando)
        quote.Subtotal += lineAfterDiscount;
        quote.Taxes += taxes;
        quote.Discounts += (lineBase - lineAfterDiscount);
        quote.Total = quote.Subtotal + quote.Taxes;
        quote.UpdatedAt = DateTime.UtcNow;
        quote.Version += 1;

        _db.QuoteDetails.Add(detail);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Sales",
            "Quote",
            quote.Id,
            "QUOTE_DETAIL_ADDED",
            string.Empty,
            $"quoteId={quote.Id}; detailId={detail.Id}; productId={detail.ProductId}; qty={detail.Quantity}; unitPrice={detail.UnitPrice}; disc={detail.DiscountPerc}; tax={detail.TaxPerc}; lineTotal={detail.LineTotal}",
            ip,
            ua,
            ct
        );

        return Result.Success(detail.Id);
    }
}
