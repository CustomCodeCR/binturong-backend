namespace Api.Endpoints.Contracts;

public sealed record CreateContractMilestoneRequest(
    string Description,
    decimal Percentage,
    decimal Amount,
    DateOnly ScheduledDate
);

public sealed record CreateContractRequest(
    string Code,
    Guid ClientId,
    Guid? QuoteId,
    Guid? SalesOrderId,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status,
    string Description,
    string Notes,
    IReadOnlyList<CreateContractMilestoneRequest> Milestones
);

public sealed record UpdateContractRequest(
    string Code,
    Guid ClientId,
    Guid? QuoteId,
    Guid? SalesOrderId,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status,
    string Description,
    string Notes
);

// Milestones
public sealed record AddContractMilestoneRequest(
    string Description,
    decimal Percentage,
    decimal Amount,
    DateOnly ScheduledDate
);

public sealed record UpdateContractMilestoneRequest(
    string Description,
    decimal Percentage,
    decimal Amount,
    DateOnly ScheduledDate,
    bool IsBilled,
    Guid? InvoiceId
);
