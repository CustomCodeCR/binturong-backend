using SharedKernel;

namespace Domain.EmployeeHistory;

public sealed record EmployeeHistoryCreatedDomainEvent(Guid HistoryId) : IDomainEvent;
