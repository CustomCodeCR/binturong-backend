using SharedKernel;

namespace Domain.InvoiceDetails;

public sealed record InvoiceDetailCreatedDomainEvent(Guid InvoiceDetailId) : IDomainEvent;
