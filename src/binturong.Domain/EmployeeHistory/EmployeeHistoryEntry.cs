using SharedKernel;

namespace Domain.EmployeeHistory;

public sealed class EmployeeHistoryEntry : Entity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }

    public Domain.Employees.Employee? Employee { get; set; }
}
