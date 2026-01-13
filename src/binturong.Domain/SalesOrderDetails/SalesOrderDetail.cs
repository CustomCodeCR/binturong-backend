using SharedKernel;

namespace Domain.SalesOrderDetails;

public sealed class SalesOrderDetail : Entity
{
    public Guid Id { get; set; }
    public Guid SalesOrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPerc { get; set; }
    public decimal TaxPerc { get; set; }
    public decimal LineTotal { get; set; }

    public Domain.SalesOrders.SalesOrder? SalesOrder { get; set; }
    public Domain.Products.Product? Product { get; set; }
}
