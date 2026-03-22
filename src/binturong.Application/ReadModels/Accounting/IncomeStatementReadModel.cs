namespace Application.ReadModels.Accounting;

public sealed class IncomeStatementReadModel
{
    public DateTime FromUtc { get; init; }
    public DateTime ToUtc { get; init; }

    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal GrossProfit { get; init; }
    public decimal NetProfit { get; init; }

    public bool HasData { get; init; }
    public string? Message { get; init; }
}
