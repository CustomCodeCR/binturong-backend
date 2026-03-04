using SharedKernel;

namespace Domain.Payments;

public sealed record PaymentCreatedDomainEvent(
    Guid PaymentId,
    Guid ClientId,
    Guid PaymentMethodId,
    DateTime PaymentDateUtc,
    decimal TotalAmount,
    string? Reference,
    string? Notes
) : IDomainEvent;

public sealed record PaymentDeletedDomainEvent(Guid PaymentId) : IDomainEvent;

public sealed record PaymentAppliedToInvoiceDomainEvent(
    Guid PaymentId,
    Guid InvoiceId,
    string? InvoiceConsecutive,
    decimal AppliedAmount,
    DateTime AtUtc
) : IDomainEvent;

public sealed record PaymentPosRejectedDomainEvent(Guid PaymentId, string Message, DateTime AtUtc)
    : IDomainEvent;
