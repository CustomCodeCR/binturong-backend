using SharedKernel;

namespace Domain.InvoiceDetails;

public sealed class InvoiceDetail : Entity
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPerc { get; set; }
    public decimal TaxPerc { get; set; }
    public decimal LineTotal { get; set; }

    public Domain.Invoices.Invoice? Invoice { get; set; }
    public Domain.Products.Product? Product { get; set; }
}
