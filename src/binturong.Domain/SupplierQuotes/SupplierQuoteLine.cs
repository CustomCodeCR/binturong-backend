using SharedKernel;

namespace Domain.SupplierQuotes;

public sealed class SupplierQuoteLine : Entity
{
    public Guid Id { get; set; }
    public Guid SupplierQuoteId { get; set; }

    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }

    public SupplierQuote? SupplierQuote { get; set; }
}
