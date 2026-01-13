namespace Application.ReadModels.Sales;

public sealed class ContractReadModel
{
    public string Id { get; init; } = default!; // "contract:{ContractId}"
    public int ContractId { get; init; }
    public string Code { get; init; } = default!;

    public int ClientId { get; init; }
    public string ClientName { get; init; } = default!;

    public int? QuoteId { get; init; }
    public int? SalesOrderId { get; init; }

    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    public string Status { get; init; } = default!;
    public string? Description { get; init; }
    public string? Notes { get; init; }

    public IReadOnlyList<ContractMilestoneReadModel> Milestones { get; init; } = [];
}

public sealed class ContractMilestoneReadModel
{
    public int MilestoneId { get; init; }
    public string Description { get; init; } = default!;
    public decimal Percentage { get; init; }
    public decimal Amount { get; init; }
    public DateTime ScheduledDate { get; init; }
    public bool IsBilled { get; init; }
    public int? InvoiceId { get; init; }
}
