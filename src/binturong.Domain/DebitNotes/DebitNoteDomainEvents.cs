using SharedKernel;

namespace Domain.DebitNotes;

public sealed record DebitNoteIssuedDomainEvent(Guid DebitNoteId) : IDomainEvent;
