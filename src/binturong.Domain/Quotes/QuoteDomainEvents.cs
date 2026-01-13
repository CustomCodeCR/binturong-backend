using SharedKernel;

namespace Domain.Quotes;

public sealed record QuoteCreatedDomainEvent(Guid QuoteId) : IDomainEvent;
