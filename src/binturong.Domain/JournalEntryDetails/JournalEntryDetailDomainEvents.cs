using SharedKernel;

namespace Domain.JournalEntryDetails;

public sealed record JournalEntryDetailCreatedDomainEvent(Guid JournalEntryDetailId) : IDomainEvent;
