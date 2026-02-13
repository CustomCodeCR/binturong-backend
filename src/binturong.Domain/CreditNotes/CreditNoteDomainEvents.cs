using SharedKernel;

namespace Domain.CreditNotes;

// CRUD
public sealed record CreditNoteCreatedDomainEvent(
    Guid CreditNoteId,
    Guid InvoiceId,
    string Reason,
    decimal TotalAmount,
    DateTime IssueDateUtc
) : IDomainEvent;

public sealed record CreditNoteUpdatedDomainEvent(
    Guid CreditNoteId,
    Guid InvoiceId,
    string Reason,
    decimal TotalAmount,
    string TaxStatus,
    string TaxKey,
    string Consecutive,
    string PdfS3Key,
    string XmlS3Key,
    DateTime UpdatedAtUtc
) : IDomainEvent;

public sealed record CreditNoteDeletedDomainEvent(Guid CreditNoteId) : IDomainEvent;

// E-invoicing lifecycle
public sealed record CreditNoteContingencyActivatedDomainEvent(Guid CreditNoteId, DateTime AtUtc)
    : IDomainEvent;

public sealed record CreditNoteEmittedDomainEvent(
    Guid CreditNoteId,
    string TaxKey,
    string Consecutive,
    string PdfS3Key,
    string XmlS3Key,
    DateTime EmittedAtUtc
) : IDomainEvent;
