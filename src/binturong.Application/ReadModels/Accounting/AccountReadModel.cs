namespace Application.ReadModels.Accounting;

public sealed class AccountReadModel
{
    public string Id { get; init; } = default!; // "acct:{AccountId}"
    public int AccountId { get; init; }

    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public int Level { get; init; }

    public int? ParentAccountId { get; init; }
    public int? CostCenterId { get; init; }
    public string Status { get; init; } = default!;
}
