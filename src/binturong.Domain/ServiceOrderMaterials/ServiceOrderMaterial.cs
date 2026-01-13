using SharedKernel;

namespace Domain.ServiceOrderMaterials;

public sealed class ServiceOrderMaterial : Entity
{
    public Guid Id { get; set; }
    public Guid ServiceOrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal EstimatedCost { get; set; }

    public Domain.ServiceOrders.ServiceOrder? ServiceOrder { get; set; }
    public Domain.Products.Product? Product { get; set; }
}
