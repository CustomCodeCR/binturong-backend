using SharedKernel;

namespace Domain.Taxes;

public sealed record TaxCreatedDomainEvent(Guid TaxId) : IDomainEvent;
