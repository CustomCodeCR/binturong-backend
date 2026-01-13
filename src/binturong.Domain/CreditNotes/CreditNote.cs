using SharedKernel;

namespace Domain.CreditNotes;

public sealed class CreditNote : Entity
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string TaxKey { get; set; } = string.Empty;
    public string Consecutive { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string TaxStatus { get; set; } = string.Empty;

    public Domain.Invoices.Invoice? Invoice { get; set; }
}
