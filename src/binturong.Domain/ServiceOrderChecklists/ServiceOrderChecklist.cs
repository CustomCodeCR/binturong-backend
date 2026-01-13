using SharedKernel;

namespace Domain.ServiceOrderChecklists;

public sealed class ServiceOrderChecklist : Entity
{
    public Guid Id { get; set; }
    public Guid ServiceOrderId { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }

    public Domain.ServiceOrders.ServiceOrder? ServiceOrder { get; set; }
}
