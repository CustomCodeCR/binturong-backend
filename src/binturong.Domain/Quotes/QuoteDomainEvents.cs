using SharedKernel;

namespace Domain.Quotes;

public sealed record QuoteCreatedDomainEvent(
    Guid QuoteId,
    string Code,
    Guid ClientId,
    Guid? BranchId,
    DateTime IssueDate,
    DateTime ValidUntil,
    string Status,
    string Currency,
    decimal ExchangeRate,
    decimal Subtotal,
    decimal Taxes,
    decimal Discounts,
    decimal Total,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record QuoteDetailAddedDomainEvent(
    Guid QuoteId,
    Guid QuoteDetailId,
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc,
    decimal LineTotal
) : IDomainEvent;

public sealed record QuoteSentDomainEvent(Guid QuoteId, DateTime SentAt) : IDomainEvent;

public sealed record QuoteAcceptedDomainEvent(Guid QuoteId, DateTime AcceptedAt) : IDomainEvent;

public sealed record QuoteRejectedDomainEvent(Guid QuoteId, string Reason, DateTime RejectedAt)
    : IDomainEvent;

public sealed record QuoteExpiredDomainEvent(Guid QuoteId, DateTime ExpiredAt) : IDomainEvent;
