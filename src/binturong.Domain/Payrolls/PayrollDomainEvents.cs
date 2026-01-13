using SharedKernel;

namespace Domain.Payrolls;

public sealed record PayrollCreatedDomainEvent(Guid PayrollId) : IDomainEvent;
