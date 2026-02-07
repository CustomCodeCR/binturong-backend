using SharedKernel;

namespace Domain.QuoteDetails;

public sealed record QuoteDetailCreatedDomainEvent(
    Guid QuoteDetailId,
    Guid QuoteId,
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal
) : IDomainEvent;
