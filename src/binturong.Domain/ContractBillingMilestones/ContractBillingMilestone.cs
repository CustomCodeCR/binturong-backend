using SharedKernel;

namespace Domain.ContractBillingMilestones;

public sealed class ContractBillingMilestone : Entity
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public bool IsBilled { get; set; }
    public Guid? InvoiceId { get; set; }

    public Domain.Contracts.Contract? Contract { get; set; }
    public Domain.Invoices.Invoice? Invoice { get; set; }
}
