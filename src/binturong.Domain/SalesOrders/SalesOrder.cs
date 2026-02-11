using Domain.SalesOrderDetails;
using SharedKernel;

namespace Domain.SalesOrders;

public sealed class SalesOrder : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid? QuoteId { get; set; }
    public Guid ClientId { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Taxes { get; set; }
    public decimal Discounts { get; set; }
    public decimal Total { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Domain.Quotes.Quote? Quote { get; set; }
    public Domain.Clients.Client? Client { get; set; }
    public Domain.Branches.Branch? Branch { get; set; }

    public ICollection<Domain.SalesOrderDetails.SalesOrderDetail> Details { get; set; } =
        new List<Domain.SalesOrderDetails.SalesOrderDetail>();
    public ICollection<Domain.Invoices.Invoice> Invoices { get; set; } =
        new List<Domain.Invoices.Invoice>();
    public ICollection<Domain.Contracts.Contract> Contracts { get; set; } =
        new List<Domain.Contracts.Contract>();
    public ICollection<Domain.PurchaseOrders.PurchaseOrder> PurchaseOrders { get; set; } =
        new List<Domain.PurchaseOrders.PurchaseOrder>();

    public void RaiseCreated() =>
        Raise(
            new SalesOrderCreatedDomainEvent(
                Id,
                Code,
                ClientId,
                BranchId,
                OrderDate,
                Status,
                Currency,
                ExchangeRate,
                Subtotal,
                Taxes,
                Discounts,
                Total,
                QuoteId
            )
        );

    public void RaiseConvertedFromQuote(Guid quoteId) =>
        Raise(new SalesOrderConvertedFromQuoteDomainEvent(Id, quoteId));

    public void RaiseConfirmed(Guid sellerUserId) =>
        Raise(new SalesOrderConfirmedDomainEvent(Id, sellerUserId, Total));

    public void RaiseDetailAdded(SalesOrderDetail d) =>
        Raise(
            new SalesOrderDetailAddedDomainEvent(
                Id,
                d.Id,
                d.ProductId,
                d.Quantity,
                d.UnitPrice,
                d.DiscountPerc,
                d.TaxPerc,
                d.LineTotal
            )
        );
}
