using SharedKernel;

namespace Domain.SupplierContacts;

public sealed record SupplierContactCreatedDomainEvent(Guid ContactId) : IDomainEvent;
