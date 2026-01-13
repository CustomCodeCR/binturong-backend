using SharedKernel;

namespace Domain.ServiceOrderServices;

public sealed class ServiceOrderService : Entity
{
    public Guid Id { get; set; }
    public Guid ServiceOrderId { get; set; }
    public Guid ServiceId { get; set; }
    public decimal Quantity { get; set; }
    public decimal RateApplied { get; set; }
    public decimal LineTotal { get; set; }

    public Domain.ServiceOrders.ServiceOrder? ServiceOrder { get; set; }
    public Domain.Services.Service? Service { get; set; }
}
