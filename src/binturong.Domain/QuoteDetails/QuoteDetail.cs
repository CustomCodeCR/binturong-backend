using SharedKernel;

namespace Domain.QuoteDetails;

public sealed class QuoteDetail : Entity
{
    public Guid Id { get; set; }
    public Guid QuoteId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPerc { get; set; }
    public decimal TaxPerc { get; set; }
    public decimal LineTotal { get; set; }

    public Domain.Quotes.Quote? Quote { get; set; }
    public Domain.Products.Product? Product { get; set; }
}
