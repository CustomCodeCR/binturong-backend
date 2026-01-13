using SharedKernel;

namespace Domain.Contracts;

public sealed class Contract : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid? QuoteId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public Domain.Clients.Client? Client { get; set; }
    public Domain.Quotes.Quote? Quote { get; set; }
    public Domain.SalesOrders.SalesOrder? SalesOrder { get; set; }

    public ICollection<Domain.ContractBillingMilestones.ContractBillingMilestone> BillingMilestones { get; set; } =
        new List<Domain.ContractBillingMilestones.ContractBillingMilestone>();

    public ICollection<Domain.Invoices.Invoice> Invoices { get; set; } =
        new List<Domain.Invoices.Invoice>();
    public ICollection<Domain.ServiceOrders.ServiceOrder> ServiceOrders { get; set; } =
        new List<Domain.ServiceOrders.ServiceOrder>();
}
