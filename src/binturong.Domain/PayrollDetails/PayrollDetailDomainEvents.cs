using SharedKernel;

namespace Domain.PayrollDetails;

public sealed record PayrollDetailCreatedDomainEvent(Guid PayrollDetailId) : IDomainEvent;
