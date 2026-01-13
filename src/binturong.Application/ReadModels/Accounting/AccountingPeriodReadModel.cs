namespace Application.ReadModels.Accounting;

public sealed class AccountingPeriodReadModel
{
    public string Id { get; init; } = default!; // "period:{PeriodId}"
    public int PeriodId { get; init; }

    public int Year { get; init; }
    public int Month { get; init; }

    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    public string Status { get; init; } = default!;
}
