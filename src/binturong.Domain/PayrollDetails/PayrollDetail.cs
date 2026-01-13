using SharedKernel;

namespace Domain.PayrollDetails;

public sealed class PayrollDetail : Entity
{
    public Guid Id { get; set; }
    public Guid PayrollId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal Deductions { get; set; }
    public decimal EmployerContrib { get; set; }
    public decimal NetSalary { get; set; }

    public Domain.Payrolls.Payroll? Payroll { get; set; }
    public Domain.Employees.Employee? Employee { get; set; }
}
