using SharedKernel;

namespace Domain.Contracts;

public sealed record ContractCreatedDomainEvent(Guid ContractId) : IDomainEvent;
