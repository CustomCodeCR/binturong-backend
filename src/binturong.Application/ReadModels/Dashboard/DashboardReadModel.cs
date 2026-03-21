namespace Application.ReadModels.Dashboard;

public sealed class DashboardReadModel
{
    public DateTime LastUpdatedAtUtc { get; init; }

    public DashboardMainIndicatorsReadModel MainIndicators { get; init; } = default!;
    public DashboardMonthlySalesReadModel MonthlySales { get; init; } = default!;
    public DashboardContractsReadModel Contracts { get; init; } = default!;
    public DashboardCriticalInventoryReadModel CriticalInventory { get; init; } = default!;
}

public sealed class DashboardMainIndicatorsReadModel
{
    public decimal MonthlySalesTotal { get; init; }
    public int ActiveContractsCount { get; init; }
    public int CriticalInventoryCount { get; init; }
}

public sealed class DashboardMonthlySalesReadModel
{
    public int Year { get; init; }
    public int Month { get; init; }

    public decimal TotalSalesAmount { get; init; }
    public int SalesOrdersCount { get; init; }
    public decimal ServicesSoldQuantity { get; init; }

    public bool HasRecords { get; init; }
    public string? Message { get; init; }
}

public sealed class DashboardContractsReadModel
{
    public int ActiveContractsCount { get; init; }
    public int ExpiringSoonCount { get; init; }
    public int ExpiredCount { get; init; }

    public bool HasActiveContracts { get; init; }
    public string? Message { get; init; }
}

public sealed class DashboardCriticalInventoryReadModel
{
    public int CriticalProductsCount { get; init; }
    public bool HasAlerts { get; init; }
    public string? Message { get; init; }

    public IReadOnlyList<DashboardCriticalInventoryItemReadModel> Items { get; init; } = [];
}

public sealed class DashboardCriticalInventoryItemReadModel
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;

    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }

    public Guid WarehouseId { get; init; }
    public string WarehouseCode { get; init; } = default!;
    public string WarehouseName { get; init; } = default!;
    public Guid BranchId { get; init; }

    public decimal CurrentStock { get; init; }
    public decimal MinStock { get; init; }
    public decimal MaxStock { get; init; }
}
