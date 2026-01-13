using SharedKernel;

namespace Domain.AccountsPayable;

public sealed record AccountPayableCreatedDomainEvent(Guid AccountPayableId) : IDomainEvent;
