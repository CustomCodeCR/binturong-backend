using SharedKernel;

namespace Domain.SupplierQuotes;

public sealed record SupplierQuoteSentDomainEvent(
    Guid SupplierQuoteId,
    string Code,
    Guid SupplierId,
    Guid? BranchId,
    DateTime RequestedAtUtc,
    string Status,
    string? Notes
) : IDomainEvent;

public sealed record SupplierQuoteLineAddedDomainEvent(
    Guid SupplierQuoteId,
    Guid SupplierQuoteLineId,
    Guid ProductId,
    decimal Quantity
) : IDomainEvent;

public sealed record SupplierQuoteRespondedDomainEvent(
    Guid SupplierQuoteId,
    DateTime RespondedAtUtc,
    string Status,
    string? SupplierMessage
) : IDomainEvent;

public sealed record SupplierQuoteResponseLineRegisteredDomainEvent(
    Guid SupplierQuoteId,
    Guid ProductId,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc,
    string? Conditions
) : IDomainEvent;

public sealed record SupplierQuoteRejectedDomainEvent(
    Guid SupplierQuoteId,
    DateTime RejectedAtUtc,
    string Status,
    string Reason
) : IDomainEvent;
