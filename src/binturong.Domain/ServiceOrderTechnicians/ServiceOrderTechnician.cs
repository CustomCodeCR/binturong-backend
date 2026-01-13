using SharedKernel;

namespace Domain.ServiceOrderTechnicians;

public sealed class ServiceOrderTechnician : Entity
{
    public Guid Id { get; set; }
    public Guid ServiceOrderId { get; set; }
    public Guid EmployeeId { get; set; }
    public string TechRole { get; set; } = string.Empty;

    public Domain.ServiceOrders.ServiceOrder? ServiceOrder { get; set; }
    public Domain.Employees.Employee? Employee { get; set; }
}
