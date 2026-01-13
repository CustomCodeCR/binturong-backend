using SharedKernel;

namespace Domain.Quotes;

public sealed class Quote : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ValidUntil { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Taxes { get; set; }
    public decimal Discounts { get; set; }
    public decimal Total { get; set; }
    public bool AcceptedByClient { get; set; }
    public DateTime? AcceptanceDate { get; set; }
    public int Version { get; set; }
    public string Notes { get; set; } = string.Empty;

    public Domain.Clients.Client? Client { get; set; }
    public Domain.Branches.Branch? Branch { get; set; }

    public ICollection<Domain.QuoteDetails.QuoteDetail> Details { get; set; } =
        new List<Domain.QuoteDetails.QuoteDetail>();
    public ICollection<Domain.Contracts.Contract> Contracts { get; set; } =
        new List<Domain.Contracts.Contract>();
    public ICollection<Domain.SalesOrders.SalesOrder> SalesOrders { get; set; } =
        new List<Domain.SalesOrders.SalesOrder>();
}
