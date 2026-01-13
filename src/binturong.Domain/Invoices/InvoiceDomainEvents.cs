using SharedKernel;

namespace Domain.Invoices;

public sealed record InvoiceIssuedDomainEvent(Guid InvoiceId) : IDomainEvent;
