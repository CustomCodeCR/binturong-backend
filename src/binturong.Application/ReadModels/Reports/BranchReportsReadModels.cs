namespace Application.ReadModels.Reports;

public sealed class BranchSalesReportReadModel
{
    public Guid BranchId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }

    public int OrdersCount { get; init; }
    public decimal TotalSales { get; init; }
    public decimal AverageOrder { get; init; }

    public IReadOnlyList<BranchSalesByDayReadModel> ByDay { get; init; } = [];
    public IReadOnlyList<BranchSalesByCurrencyReadModel> ByCurrency { get; init; } = [];
}

public sealed class BranchSalesByDayReadModel
{
    public DateTime DayUtc { get; init; }
    public int OrdersCount { get; init; }
    public decimal TotalSales { get; init; }
}

public sealed class BranchSalesByCurrencyReadModel
{
    public string Currency { get; init; } = default!;
    public int OrdersCount { get; init; }
    public decimal TotalSales { get; init; }
}

public sealed class BranchComparisonReportReadModel
{
    public Guid BranchAId { get; init; }
    public Guid BranchBId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }

    public BranchSalesReportReadModel BranchA { get; init; } = default!;
    public BranchSalesReportReadModel BranchB { get; init; } = default!;

    public decimal TotalSalesDiff { get; init; }
    public decimal TotalSalesDiffPerc { get; init; }

    public int OrdersCountDiff { get; init; }
    public decimal OrdersCountDiffPerc { get; init; }
}
