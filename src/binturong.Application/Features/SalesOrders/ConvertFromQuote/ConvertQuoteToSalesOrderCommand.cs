using Application.Abstractions.Messaging;

namespace Application.Features.SalesOrders.ConvertFromQuote;

public sealed record ConvertQuoteToSalesOrderCommand(
    Guid QuoteId,
    Guid? BranchId,
    string Currency,
    decimal ExchangeRate,
    string? Notes
) : ICommand<Guid>;
