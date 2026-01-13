using SharedKernel;

namespace Domain.PaymentDetails;

public sealed record PaymentAppliedToInvoiceDomainEvent(Guid PaymentDetailId) : IDomainEvent;
