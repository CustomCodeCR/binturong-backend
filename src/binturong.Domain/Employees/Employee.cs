using SharedKernel;

namespace Domain.Employees;

public sealed class Employee : Entity
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? BranchId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public DateOnly HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public bool IsActive { get; set; }

    // Navigation
    public Domain.Users.User? User { get; set; }
    public Domain.Branches.Branch? Branch { get; set; }
    public ICollection<Domain.EmployeeHistory.EmployeeHistoryEntry> History { get; set; } =
        new List<Domain.EmployeeHistory.EmployeeHistoryEntry>();

    public ICollection<Domain.PurchaseRequests.PurchaseRequest> PurchaseRequests { get; set; } =
        new List<Domain.PurchaseRequests.PurchaseRequest>();
    public ICollection<Domain.PayrollDetails.PayrollDetail> PayrollDetails { get; set; } =
        new List<Domain.PayrollDetails.PayrollDetail>();
    public ICollection<Domain.ServiceOrderTechnicians.ServiceOrderTechnician> ServiceOrderTechnicians { get; set; } =
        new List<Domain.ServiceOrderTechnicians.ServiceOrderTechnician>();
}
