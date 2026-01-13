using SharedKernel;

namespace Domain.ClientContacts;

public sealed record ClientContactCreatedDomainEvent(Guid ContactId) : IDomainEvent;
