using Application.Abstractions.Messaging;
using Application.ReadModels.Dashboard;
using Application.ReadModels.Inventory;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Dashboard.GetDashboard;

internal sealed class GetDashboardQueryHandler
    : IQueryHandler<GetDashboardQuery, DashboardReadModel>
{
    private readonly IMongoDatabase _db;

    public GetDashboardQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<DashboardReadModel>> Handle(
        GetDashboardQuery query,
        CancellationToken ct
    )
    {
        var salesOrdersCol = _db.GetCollection<SalesOrderReadModel>(
            Application.ReadModels.Common.MongoCollections.SalesOrders
        );
        var contractsCol = _db.GetCollection<ContractReadModel>(
            Application.ReadModels.Common.MongoCollections.Contracts
        );
        var stockCol = _db.GetCollection<ProductStockReadModel>(
            Application.ReadModels.Common.MongoCollections.ProductStocks
        );

        var nowUtc = DateTime.UtcNow;
        var monthStartUtc = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStartUtc = monthStartUtc.AddMonths(1);

        var monthlySales = await LoadMonthlySalesAsync(
            salesOrdersCol,
            monthStartUtc,
            nextMonthStartUtc,
            query.BranchId,
            ct
        );

        var contracts = await LoadContractsAsync(contractsCol, nowUtc, ct);

        var criticalInventory = await LoadCriticalInventoryAsync(stockCol, query.BranchId, ct);

        var dashboard = new DashboardReadModel
        {
            LastUpdatedAtUtc = nowUtc,
            MainIndicators = new DashboardMainIndicatorsReadModel
            {
                MonthlySalesTotal = monthlySales.TotalSalesAmount,
                ActiveContractsCount = contracts.ActiveContractsCount,
                CriticalInventoryCount = criticalInventory.CriticalProductsCount,
            },
            MonthlySales = monthlySales,
            Contracts = contracts,
            CriticalInventory = criticalInventory,
        };

        return Result.Success(dashboard);
    }

    private static async Task<DashboardMonthlySalesReadModel> LoadMonthlySalesAsync(
        IMongoCollection<SalesOrderReadModel> col,
        DateTime monthStartUtc,
        DateTime nextMonthStartUtc,
        Guid? branchId,
        CancellationToken ct
    )
    {
        var builder = Builders<SalesOrderReadModel>.Filter;
        var filter =
            builder.Gte(x => x.OrderDate, monthStartUtc)
            & builder.Lt(x => x.OrderDate, nextMonthStartUtc);

        if (branchId.HasValue)
            filter &= builder.Eq(x => x.BranchId, branchId.Value);

        var docs = await col.Find(filter).ToListAsync(ct);

        if (docs.Count == 0)
        {
            return new DashboardMonthlySalesReadModel
            {
                Year = monthStartUtc.Year,
                Month = monthStartUtc.Month,
                TotalSalesAmount = 0m,
                SalesOrdersCount = 0,
                ServicesSoldQuantity = 0m,
                HasRecords = false,
                Message = "Sin registros",
            };
        }

        var servicesSold = docs.SelectMany(x => x.Lines ?? [])
            .Where(x => string.Equals(x.ItemType, "Service", StringComparison.OrdinalIgnoreCase))
            .Sum(x => x.Quantity);

        return new DashboardMonthlySalesReadModel
        {
            Year = monthStartUtc.Year,
            Month = monthStartUtc.Month,
            TotalSalesAmount = docs.Sum(x => x.Total),
            SalesOrdersCount = docs.Count,
            ServicesSoldQuantity = servicesSold,
            HasRecords = true,
            Message = null,
        };
    }

    private static async Task<DashboardContractsReadModel> LoadContractsAsync(
        IMongoCollection<ContractReadModel> col,
        DateTime nowUtc,
        CancellationToken ct
    )
    {
        var docs = await col.Find(Builders<ContractReadModel>.Filter.Empty).ToListAsync(ct);

        var today = nowUtc.Date;

        var activeContracts = docs.Where(x =>
                string.Equals(x.Status, "Active", StringComparison.OrdinalIgnoreCase)
            )
            .ToList();

        var expiredCount = docs.Count(x => x.EndDate.HasValue && x.EndDate.Value.Date < today);

        var expiringSoonCount = activeContracts.Count(x =>
            x.EndDate.HasValue
            && x.EndDate.Value.Date >= today
            && x.EndDate.Value.Date <= today.AddDays(30)
        );

        if (activeContracts.Count == 0)
        {
            return new DashboardContractsReadModel
            {
                ActiveContractsCount = 0,
                ExpiringSoonCount = expiringSoonCount,
                ExpiredCount = expiredCount,
                HasActiveContracts = false,
                Message = "No hay contratos activos",
            };
        }

        return new DashboardContractsReadModel
        {
            ActiveContractsCount = activeContracts.Count,
            ExpiringSoonCount = expiringSoonCount,
            ExpiredCount = expiredCount,
            HasActiveContracts = true,
            Message = null,
        };
    }

    private static async Task<DashboardCriticalInventoryReadModel> LoadCriticalInventoryAsync(
        IMongoCollection<ProductStockReadModel> col,
        Guid? branchId,
        CancellationToken ct
    )
    {
        var builder = Builders<ProductStockReadModel>.Filter;
        var filter = builder.Empty;

        var products = await col.Find(filter).ToListAsync(ct);

        var criticalItems = products
            .SelectMany(product =>
                product.Warehouses.Select(warehouse => new DashboardCriticalInventoryItemReadModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    WarehouseId = warehouse.WarehouseId,
                    WarehouseCode = warehouse.WarehouseCode,
                    WarehouseName = warehouse.WarehouseName,
                    BranchId = warehouse.BranchId,
                    CurrentStock = warehouse.CurrentStock,
                    MinStock = warehouse.MinStock,
                    MaxStock = warehouse.MaxStock,
                })
            )
            .Where(x => x.CurrentStock <= x.MinStock)
            .Where(x => !branchId.HasValue || x.BranchId == branchId.Value)
            .OrderBy(x => x.CurrentStock)
            .ThenBy(x => x.ProductName)
            .ToList();

        if (criticalItems.Count == 0)
        {
            return new DashboardCriticalInventoryReadModel
            {
                CriticalProductsCount = 0,
                HasAlerts = false,
                Message = "Sin alertas",
                Items = [],
            };
        }

        var distinctProducts = criticalItems.Select(x => x.ProductId).Distinct().Count();

        return new DashboardCriticalInventoryReadModel
        {
            CriticalProductsCount = distinctProducts,
            HasAlerts = true,
            Message = null,
            Items = criticalItems,
        };
    }
}
