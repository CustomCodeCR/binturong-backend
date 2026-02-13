using SharedKernel;

namespace Domain.Contracts;

public sealed record ContractCreatedDomainEvent(
    Guid ContractId,
    string Code,
    Guid ClientId,
    Guid? QuoteId,
    Guid? SalesOrderId,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status,
    string Description,
    string Notes
) : IDomainEvent;

public sealed record ContractUpdatedDomainEvent(
    Guid ContractId,
    string Code,
    Guid ClientId,
    Guid? QuoteId,
    Guid? SalesOrderId,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status,
    string Description,
    string Notes
) : IDomainEvent;

public sealed record ContractDeletedDomainEvent(Guid ContractId) : IDomainEvent;

public sealed record ContractRenewedDomainEvent(
    Guid ContractId,
    DateOnly NewStartDate,
    DateOnly NewEndDate,
    DateTime RenewedAtUtc
) : IDomainEvent;

public sealed record ContractCreatedFromQuoteDomainEvent(
    Guid ContractId,
    Guid QuoteId,
    DateOnly StartDate,
    DateOnly? EndDate,
    Guid ResponsibleUserId,
    DateTime CreatedAtUtc
) : IDomainEvent;

public sealed record ContractExpiryNoticeSentDomainEvent(
    Guid ContractId,
    DateOnly? EndDate,
    int NoticeDays,
    DateTime NotifiedAtUtc
) : IDomainEvent;
