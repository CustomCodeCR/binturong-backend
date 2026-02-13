using SharedKernel;

namespace Domain.DebitNotes;

// CRUD
public sealed record DebitNoteCreatedDomainEvent(
    Guid DebitNoteId,
    Guid InvoiceId,
    string Reason,
    decimal TotalAmount,
    DateTime IssueDateUtc
) : IDomainEvent;

public sealed record DebitNoteUpdatedDomainEvent(
    Guid DebitNoteId,
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

public sealed record DebitNoteDeletedDomainEvent(Guid DebitNoteId) : IDomainEvent;

// E-invoicing lifecycle
public sealed record DebitNoteContingencyActivatedDomainEvent(Guid DebitNoteId, DateTime AtUtc)
    : IDomainEvent;

public sealed record DebitNoteEmittedDomainEvent(
    Guid DebitNoteId,
    string TaxKey,
    string Consecutive,
    string PdfS3Key,
    string XmlS3Key,
    DateTime EmittedAtUtc
) : IDomainEvent;
