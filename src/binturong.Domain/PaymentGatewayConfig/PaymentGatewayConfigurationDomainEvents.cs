using SharedKernel;

namespace Domain.PaymentGatewayConfig;

public sealed record PaymentGatewayConfiguredDomainEvent(Guid GatewayId) : IDomainEvent;
