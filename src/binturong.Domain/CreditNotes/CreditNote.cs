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

    // Draft | Processing | Emitted | Rejected | Contingency
    public string TaxStatus { get; set; } = "Draft";

    // Storage
    public string PdfS3Key { get; set; } = string.Empty;
    public string XmlS3Key { get; set; } = string.Empty;

    public Domain.Invoices.Invoice? Invoice { get; set; }

    // =========================
    // Domain events – CRUD
    // =========================
    public void RaiseCreated() =>
        Raise(new CreditNoteCreatedDomainEvent(Id, InvoiceId, Reason, TotalAmount, IssueDate));

    public void RaiseUpdated() =>
        Raise(
            new CreditNoteUpdatedDomainEvent(
                Id,
                InvoiceId,
                Reason,
                TotalAmount,
                TaxStatus,
                TaxKey,
                Consecutive,
                PdfS3Key,
                XmlS3Key,
                DateTime.UtcNow
            )
        );

    public void RaiseDeleted() => Raise(new CreditNoteDeletedDomainEvent(Id));

    // =========================
    // E-invoicing lifecycle
    // =========================
    public void ActivateContingency(DateTime nowUtc)
    {
        TaxStatus = "Contingency";
        Raise(new CreditNoteContingencyActivatedDomainEvent(Id, nowUtc));
    }

    public void MarkEmitted(
        string taxKey,
        string consecutive,
        string pdfS3Key,
        string xmlS3Key,
        DateTime nowUtc
    )
    {
        TaxKey = taxKey;
        Consecutive = consecutive;
        PdfS3Key = pdfS3Key;
        XmlS3Key = xmlS3Key;

        TaxStatus = "Emitted";

        Raise(
            new CreditNoteEmittedDomainEvent(Id, taxKey, consecutive, pdfS3Key, xmlS3Key, nowUtc)
        );
    }
}
