using SharedKernel;

namespace Domain.Suppliers;

public sealed record SupplierCreatedDomainEvent(Guid SupplierId) : IDomainEvent;
