using SharedKernel;

namespace Domain.Services;

public sealed class Service : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StandardTimeMin { get; set; }
    public decimal BaseRate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Domain.ServiceOrderServices.ServiceOrderService> ServiceOrderServices { get; set; } =
        new List<Domain.ServiceOrderServices.ServiceOrderService>();
}
