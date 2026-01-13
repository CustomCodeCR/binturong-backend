using SharedKernel;

namespace Domain.Employees;

public sealed record EmployeeCreatedDomainEvent(Guid EmployeeId) : IDomainEvent;
