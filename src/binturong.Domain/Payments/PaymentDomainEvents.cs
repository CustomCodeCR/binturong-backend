using SharedKernel;

namespace Domain.Payments;

public sealed record PaymentReceivedDomainEvent(Guid PaymentId) : IDomainEvent;
