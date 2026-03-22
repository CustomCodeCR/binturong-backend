namespace Application.ReadModels.Accounting;

public sealed class CashFlowReadModel
{
    public DateTime FromUtc { get; init; }
    public DateTime ToUtc { get; init; }

    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal Balance { get; init; }

    public IReadOnlyList<CashFlowPointReadModel> Points { get; init; } = [];
}

public sealed class CashFlowPointReadModel
{
    public DateTime DateUtc { get; init; }
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
    public decimal Balance { get; init; }
}
