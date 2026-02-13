using SharedKernel;

namespace Domain.Invoices;

// CRUD
public sealed record InvoiceCreatedDomainEvent(
    Guid InvoiceId,
    Guid ClientId,
    Guid? BranchId,
    Guid? SalesOrderId,
    Guid? ContractId,
    DateTime IssueDate,
    string DocumentType,
    string Currency,
    decimal ExchangeRate,
    decimal Subtotal,
    decimal Taxes,
    decimal Discounts,
    decimal Total
) : IDomainEvent;

public sealed record InvoiceUpdatedDomainEvent(
    Guid InvoiceId,
    Guid ClientId,
    Guid? BranchId,
    Guid? SalesOrderId,
    Guid? ContractId,
    DateTime IssueDate,
    string DocumentType,
    string Currency,
    decimal ExchangeRate,
    decimal Subtotal,
    decimal Taxes,
    decimal Discounts,
    decimal Total
) : IDomainEvent;

public sealed record InvoiceDeletedDomainEvent(Guid InvoiceId) : IDomainEvent;

// Lifecycle / e-invoicing
public sealed record InvoiceEmissionRequestedDomainEvent(
    Guid InvoiceId,
    string Mode, // "Normal" | "Contingency"
    DateTime RequestedAtUtc
) : IDomainEvent;

public sealed record InvoiceXmlGeneratedDomainEvent(
    Guid InvoiceId,
    string XmlS3Key,
    DateTime GeneratedAtUtc
) : IDomainEvent;

public sealed record InvoicePdfGeneratedDomainEvent(
    Guid InvoiceId,
    string PdfS3Key,
    DateTime GeneratedAtUtc
) : IDomainEvent;

public sealed record InvoiceSentToTaxAuthorityDomainEvent(
    Guid InvoiceId,
    string TaxKey,
    string Consecutive,
    string TaxStatus,
    DateTime SentAtUtc
) : IDomainEvent;

public sealed record InvoiceTaxAuthorityRejectedDomainEvent(
    Guid InvoiceId,
    string ErrorCode,
    string ErrorMessage,
    DateTime RejectedAtUtc
) : IDomainEvent;

public sealed record InvoiceContingencyActivatedDomainEvent(Guid InvoiceId, DateTime AtUtc)
    : IDomainEvent;

public sealed record InvoiceEmissionRejectedDomainEvent(
    Guid InvoiceId,
    string Reason,
    DateTime AtUtc
) : IDomainEvent;

public sealed record InvoiceEmittedDomainEvent(
    Guid InvoiceId,
    string TaxKey,
    string Consecutive,
    string PdfS3Key,
    string XmlS3Key,
    DateTime EmittedAtUtc
) : IDomainEvent;

public sealed record InvoiceEmailSentDomainEvent(Guid InvoiceId, string To, DateTime SentAtUtc)
    : IDomainEvent;

// A/R
public sealed record InvoicePaymentAppliedDomainEvent(
    Guid InvoiceId,
    decimal AppliedAmount,
    decimal PaidAmount,
    decimal PendingAmount,
    DateTime AtUtc
) : IDomainEvent;

public sealed record InvoicePaidDomainEvent(Guid InvoiceId, DateTime PaidAtUtc) : IDomainEvent;

// Conversions
public sealed record InvoiceCreatedFromQuoteDomainEvent(
    Guid InvoiceId,
    Guid QuoteId,
    Guid ClientId,
    DateTime CreatedAtUtc
) : IDomainEvent;

public sealed record InvoicePaymentVerificationSetDomainEvent(
    Guid InvoiceId,
    string Reason, // "BankTransferPending"
    DateTime AtUtc
) : IDomainEvent;

// Verificación completada (opcional, si luego quieres pasar de Verification -> Paid/Pending)
public sealed record InvoicePaymentVerificationClearedDomainEvent(Guid InvoiceId, DateTime AtUtc)
    : IDomainEvent;
