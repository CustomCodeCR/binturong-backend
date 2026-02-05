using SharedKernel;

namespace Domain.EmployeeHistory;

public sealed record EmployeeCheckInDomainEvent(Guid HistoryId, Guid EmployeeId, DateTime CheckInAt)
    : IDomainEvent;

public sealed record EmployeeCheckOutDomainEvent(
    Guid HistoryId,
    Guid EmployeeId,
    DateTime CheckOutAt
) : IDomainEvent;
