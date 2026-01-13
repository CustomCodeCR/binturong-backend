using SharedKernel;

namespace Domain.PaymentMethods;

public sealed record PaymentMethodCreatedDomainEvent(Guid PaymentMethodId) : IDomainEvent;
