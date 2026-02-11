namespace Application.ReadModels.Sales;

public sealed class ContractReadModel
{
    public string Id { get; init; } = default!; // "contract:{ContractId}"
    public Guid ContractId { get; init; }
    public string Code { get; init; } = default!;

    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = default!;

    public Guid? QuoteId { get; init; }
    public Guid? SalesOrderId { get; init; }

    // store DateOnly as DateTime in Mongo (UTC midnight)
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }

    public string Status { get; init; } = default!;
    public string? Description { get; init; }
    public string? Notes { get; init; }

    public IReadOnlyList<ContractMilestoneReadModel> Milestones { get; init; } = [];
}

public sealed class ContractMilestoneReadModel
{
    public Guid MilestoneId { get; init; }
    public string Description { get; init; } = default!;
    public decimal Percentage { get; init; }
    public decimal Amount { get; init; }
    public DateTime ScheduledDate { get; init; }
    public bool IsBilled { get; init; }
    public Guid? InvoiceId { get; init; }
}
