using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Reports;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Branches.Reports.GetBranchSalesReport;

internal sealed class GetBranchSalesReportQueryHandler
    : IQueryHandler<GetBranchSalesReportQuery, BranchSalesReportReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetBranchSalesReportQueryHandler(
        IMongoDatabase db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<BranchSalesReportReadModel>> Handle(
        GetBranchSalesReportQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.BranchId, q.BranchId);

        if (q.From is not null)
            filter &= Builders<SalesOrderReadModel>.Filter.Gte(x => x.OrderDate, q.From.Value);

        if (q.To is not null)
            filter &= Builders<SalesOrderReadModel>.Filter.Lte(x => x.OrderDate, q.To.Value);

        if (!string.IsNullOrWhiteSpace(q.Status))
            filter &= Builders<SalesOrderReadModel>.Filter.Eq(x => x.Status, q.Status.Trim());

        var docs = await col.Find(filter).ToListAsync(ct);

        var ordersCount = docs.Count;
        var totalSales = docs.Sum(x => x.Total);
        var avg = ordersCount == 0 ? 0m : totalSales / ordersCount;

        var byDay = docs.GroupBy(x => new DateTime(
                x.OrderDate.Year,
                x.OrderDate.Month,
                x.OrderDate.Day,
                0,
                0,
                0,
                DateTimeKind.Utc
            ))
            .OrderBy(g => g.Key)
            .Select(g => new BranchSalesByDayReadModel
            {
                DayUtc = g.Key,
                OrdersCount = g.Count(),
                TotalSales = g.Sum(x => x.Total),
            })
            .ToList();

        var byCurrency = docs.GroupBy(x => (x.Currency ?? string.Empty).Trim())
            .OrderBy(g => g.Key)
            .Select(g => new BranchSalesByCurrencyReadModel
            {
                Currency = string.IsNullOrWhiteSpace(g.Key) ? "N/A" : g.Key,
                OrdersCount = g.Count(),
                TotalSales = g.Sum(x => x.Total),
            })
            .ToList();

        var rm = new BranchSalesReportReadModel
        {
            BranchId = q.BranchId,
            From = q.From,
            To = q.To,
            OrdersCount = ordersCount,
            TotalSales = totalSales,
            AverageOrder = avg,
            ByDay = byDay,
            ByCurrency = byCurrency,
        };

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Branches",
            "BranchReport",
            q.BranchId,
            "BRANCH_SALES_REPORT_READ",
            string.Empty,
            $"branchId={q.BranchId}; from={q.From}; to={q.To}; status={q.Status}; orders={ordersCount}; total={totalSales}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(rm);
    }
}
