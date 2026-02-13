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

    // E-invoicing status
    public string TaxStatus { get; set; } = "Draft"; // Draft | Processing | Emitted | Rejected | Contingency
    public string InternalStatus { get; set; } = "Draft"; // Draft | Ok | Error | PendingResend | Pending | Paid

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

    // =========================
    // Domain events – CRUD
    // =========================
    public void RaiseCreated() =>
        Raise(
            new InvoiceCreatedDomainEvent(
                Id,
                ClientId,
                BranchId,
                SalesOrderId,
                ContractId,
                IssueDate,
                DocumentType,
                Currency,
                ExchangeRate,
                Subtotal,
                Taxes,
                Discounts,
                Total
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new InvoiceUpdatedDomainEvent(
                Id,
                ClientId,
                BranchId,
                SalesOrderId,
                ContractId,
                IssueDate,
                DocumentType,
                Currency,
                ExchangeRate,
                Subtotal,
                Taxes,
                Discounts,
                Total
            )
        );

    public void RaiseDeleted() => Raise(new InvoiceDeletedDomainEvent(Id));

    // =========================
    // E-invoicing lifecycle
    // =========================
    public void RaiseCreatedFromQuote(Guid quoteId, DateTime nowUtc) =>
        Raise(new InvoiceCreatedFromQuoteDomainEvent(Id, quoteId, ClientId, nowUtc));

    public void RaiseEmissionRequested(string mode, DateTime nowUtc) =>
        Raise(new InvoiceEmissionRequestedDomainEvent(Id, mode, nowUtc));

    public void RaiseXmlGenerated(string xmlS3Key, DateTime nowUtc) =>
        Raise(new InvoiceXmlGeneratedDomainEvent(Id, xmlS3Key, nowUtc));

    public void RaisePdfGenerated(string pdfS3Key, DateTime nowUtc) =>
        Raise(new InvoicePdfGeneratedDomainEvent(Id, pdfS3Key, nowUtc));

    public void RaiseSentToTaxAuthority(
        string taxKey,
        string consecutive,
        string taxStatus,
        DateTime nowUtc
    ) =>
        Raise(new InvoiceSentToTaxAuthorityDomainEvent(Id, taxKey, consecutive, taxStatus, nowUtc));

    public void RaiseTaxAuthorityRejected(string errorCode, string errorMessage, DateTime nowUtc) =>
        Raise(new InvoiceTaxAuthorityRejectedDomainEvent(Id, errorCode, errorMessage, nowUtc));

    public void RaiseEmailSent(string to, DateTime nowUtc) =>
        Raise(new InvoiceEmailSentDomainEvent(Id, to, nowUtc));

    // Called by ElectronicInvoicingService
    public void RaiseContingencyActivated(DateTime nowUtc)
    {
        TaxStatus = "Contingency";
        InternalStatus = "PendingResend";
        Raise(new InvoiceContingencyActivatedDomainEvent(Id, nowUtc));
    }

    // Called by ElectronicInvoicingService
    public void RaiseEmissionRejected(string reason, DateTime nowUtc)
    {
        TaxStatus = "Rejected";
        InternalStatus = "Error";
        Raise(new InvoiceEmissionRejectedDomainEvent(Id, reason, nowUtc));
    }

    // Called by ElectronicInvoicingService
    public void RaiseEmitted(DateTime nowUtc)
    {
        // IMPORTANT: service sets TaxStatus before calling; but we normalize here too
        if (string.IsNullOrWhiteSpace(TaxStatus))
            TaxStatus = "Emitted";

        InternalStatus = "Ok";
        Raise(new InvoiceEmittedDomainEvent(Id, TaxKey, Consecutive, PdfS3Key, XmlS3Key, nowUtc));
    }

    // =========================
    // Accounts receivable (HU-FAC-03)
    // =========================
    public void ApplyPayment(decimal appliedAmount, DateTime nowUtc)
    {
        var paid = PaymentDetails.Sum(x => x.AppliedAmount);
        var pending = Math.Max(0, Total - paid);

        Raise(new InvoicePaymentAppliedDomainEvent(Id, appliedAmount, paid, pending, nowUtc));

        if (pending <= 0)
        {
            InternalStatus = "Paid";
            Raise(new InvoicePaidDomainEvent(Id, nowUtc));
        }
        else
        {
            InternalStatus = "Pending";
        }
    }
}
