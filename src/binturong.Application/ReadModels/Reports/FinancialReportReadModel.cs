namespace Application.ReadModels.Reports;

public sealed class FinancialReportReadModel
{
    public DateTime FromUtc { get; init; }
    public DateTime ToUtc { get; init; }

    public decimal SalesTotal { get; init; }
    public decimal ExpensesTotal { get; init; }
    public decimal Profit { get; init; }

    public bool HasData { get; init; }
    public string? Message { get; init; }
}
