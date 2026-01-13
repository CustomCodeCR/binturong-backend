using SharedKernel;

namespace Domain.CreditNotes;

public sealed record CreditNoteIssuedDomainEvent(Guid CreditNoteId) : IDomainEvent;
