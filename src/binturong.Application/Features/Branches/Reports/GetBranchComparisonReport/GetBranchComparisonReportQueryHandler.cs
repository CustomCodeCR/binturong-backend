using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Reports;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Branches.Reports.GetBranchComparisonReport;

internal sealed class GetBranchComparisonReportQueryHandler
    : IQueryHandler<GetBranchComparisonReportQuery, BranchComparisonReportReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetBranchComparisonReportQueryHandler(
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

    public async Task<Result<BranchComparisonReportReadModel>> Handle(
        GetBranchComparisonReportQuery q,
        CancellationToken ct
    )
    {
        if (q.BranchAId == Guid.Empty || q.BranchBId == Guid.Empty)
            return Result.Failure<BranchComparisonReportReadModel>(
                Error.Validation("Branches.Report.InvalidBranch", "Branch ids are required.")
            );

        if (q.BranchAId == q.BranchBId)
            return Result.Failure<BranchComparisonReportReadModel>(
                Error.Validation("Branches.Report.SameBranch", "Branches must be different.")
            );

        var col = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var aDocs = await Fetch(col, q.BranchAId, q.From, q.To, q.Status, ct);
        var bDocs = await Fetch(col, q.BranchBId, q.From, q.To, q.Status, ct);

        var a = Build(q.BranchAId, q.From, q.To, aDocs);
        var b = Build(q.BranchBId, q.From, q.To, bDocs);

        var totalDiff = a.TotalSales - b.TotalSales;
        var totalDiffPerc =
            b.TotalSales == 0m
                ? (a.TotalSales == 0m ? 0m : 100m)
                : (totalDiff / b.TotalSales) * 100m;

        var ordersDiff = a.OrdersCount - b.OrdersCount;
        var ordersDiffPerc =
            b.OrdersCount == 0
                ? (a.OrdersCount == 0 ? 0m : 100m)
                : ((decimal)ordersDiff / b.OrdersCount) * 100m;

        var rm = new BranchComparisonReportReadModel
        {
            BranchAId = q.BranchAId,
            BranchBId = q.BranchBId,
            From = q.From,
            To = q.To,
            BranchA = a,
            BranchB = b,
            TotalSalesDiff = totalDiff,
            TotalSalesDiffPerc = totalDiffPerc,
            OrdersCountDiff = ordersDiff,
            OrdersCountDiffPerc = ordersDiffPerc,
        };

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Branches",
            "BranchComparisonReport",
            null,
            "BRANCH_COMPARISON_REPORT_READ",
            string.Empty,
            $"branchA={q.BranchAId}; branchB={q.BranchBId}; from={q.From}; to={q.To}; status={q.Status}; aOrders={a.OrdersCount}; bOrders={b.OrdersCount}; aTotal={a.TotalSales}; bTotal={b.TotalSales}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(rm);
    }

    private static async Task<List<SalesOrderReadModel>> Fetch(
        IMongoCollection<SalesOrderReadModel> col,
        Guid branchId,
        DateTime? from,
        DateTime? to,
        string? status,
        CancellationToken ct
    )
    {
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.BranchId, branchId);

        if (from is not null)
            filter &= Builders<SalesOrderReadModel>.Filter.Gte(x => x.OrderDate, from.Value);

        if (to is not null)
            filter &= Builders<SalesOrderReadModel>.Filter.Lte(x => x.OrderDate, to.Value);

        if (!string.IsNullOrWhiteSpace(status))
            filter &= Builders<SalesOrderReadModel>.Filter.Eq(x => x.Status, status.Trim());

        return await col.Find(filter).ToListAsync(ct);
    }

    private static BranchSalesReportReadModel Build(
        Guid branchId,
        DateTime? from,
        DateTime? to,
        IReadOnlyList<SalesOrderReadModel> docs
    )
    {
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

        return new BranchSalesReportReadModel
        {
            BranchId = branchId,
            From = from,
            To = to,
            OrdersCount = ordersCount,
            TotalSales = totalSales,
            AverageOrder = avg,
            ByDay = byDay,
            ByCurrency = byCurrency,
        };
    }
}
