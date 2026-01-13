using SharedKernel;

namespace Domain.ClientAddresses;

public sealed record ClientAddressCreatedDomainEvent(Guid AddressId) : IDomainEvent;
