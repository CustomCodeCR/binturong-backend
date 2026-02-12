using SharedKernel;

namespace Domain.PayrollOvertimes;

public sealed class PayrollOvertimeEntry : Entity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public decimal Hours { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    public Domain.Employees.Employee? Employee { get; set; }

    public void RaiseRegistered() =>
        Raise(
            new PayrollOvertimeRegisteredDomainEvent(Id, EmployeeId, WorkDate, Hours, CreatedAtUtc)
        );

    public void RaiseDeleted() => Raise(new PayrollOvertimeDeletedDomainEvent(Id, EmployeeId));
}
