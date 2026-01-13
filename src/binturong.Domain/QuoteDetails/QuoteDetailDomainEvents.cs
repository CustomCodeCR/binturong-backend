using SharedKernel;

namespace Domain.QuoteDetails;

public sealed record QuoteDetailCreatedDomainEvent(Guid QuoteDetailId) : IDomainEvent;
