using SharedKernel;

namespace Domain.GatewayTransactions;

public sealed record GatewayTransactionCreatedDomainEvent(Guid GatewayTransactionId) : IDomainEvent;
