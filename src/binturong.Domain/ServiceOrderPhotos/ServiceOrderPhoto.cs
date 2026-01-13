using SharedKernel;

namespace Domain.ServiceOrderPhotos;

public sealed class ServiceOrderPhoto : Entity
{
    public Guid Id { get; set; }
    public Guid ServiceOrderId { get; set; }
    public string PhotoS3Key { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Domain.ServiceOrders.ServiceOrder? ServiceOrder { get; set; }
}
