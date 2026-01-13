using SharedKernel;

namespace Domain.Invoices;

public sealed class Invoice : Entity
{
    public Guid Id { get; set; }
    public string TaxKey { get; set; } = string.Empty;
    public string Consecutive { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public Guid? ContractId { get; set; }
    public DateTime IssueDate { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Taxes { get; set; }
    public decimal Discounts { get; set; }
    public decimal Total { get; set; }
    public string TaxStatus { get; set; } = string.Empty;
    public string InternalStatus { get; set; } = string.Empty;
    public bool EmailSent { get; set; }
    public string PdfS3Key { get; set; } = string.Empty;
    public string XmlS3Key { get; set; } = string.Empty;

    public Domain.Clients.Client? Client { get; set; }
    public Domain.Branches.Branch? Branch { get; set; }
    public Domain.SalesOrders.SalesOrder? SalesOrder { get; set; }
    public Domain.Contracts.Contract? Contract { get; set; }

    public ICollection<Domain.InvoiceDetails.InvoiceDetail> Details { get; set; } =
        new List<Domain.InvoiceDetails.InvoiceDetail>();
    public ICollection<Domain.CreditNotes.CreditNote> CreditNotes { get; set; } =
        new List<Domain.CreditNotes.CreditNote>();
    public ICollection<Domain.DebitNotes.DebitNote> DebitNotes { get; set; } =
        new List<Domain.DebitNotes.DebitNote>();
    public ICollection<Domain.PaymentDetails.PaymentDetail> PaymentDetails { get; set; } =
        new List<Domain.PaymentDetails.PaymentDetail>();
    public ICollection<Domain.GatewayTransactions.GatewayTransaction> GatewayTransactions { get; set; } =
        new List<Domain.GatewayTransactions.GatewayTransaction>();
}
