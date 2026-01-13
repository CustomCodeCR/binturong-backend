using SharedKernel;

namespace Domain.PurchaseOrderDetails;

public sealed class PurchaseOrderDetail : Entity
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPerc { get; set; }
    public decimal TaxPerc { get; set; }
    public decimal LineTotal { get; set; }

    public Domain.PurchaseOrders.PurchaseOrder? PurchaseOrder { get; set; }
    public Domain.Products.Product? Product { get; set; }
}
