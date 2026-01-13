using SharedKernel;

namespace Domain.JournalEntries;

public sealed record JournalEntryCreatedDomainEvent(Guid JournalEntryId) : IDomainEvent;
