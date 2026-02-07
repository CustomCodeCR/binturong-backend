using Application.Abstractions.Messaging;

namespace Application.Features.Quotes.AddDetail;

public sealed record AddQuoteDetailCommand(
    Guid QuoteId,
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc
) : ICommand<Guid>;
