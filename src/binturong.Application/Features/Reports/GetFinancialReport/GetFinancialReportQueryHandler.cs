using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Purchases;
using Application.ReadModels.Reports;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Reports.GetFinancialReport;

internal sealed class GetFinancialReportQueryHandler
    : IQueryHandler<GetFinancialReportQuery, FinancialReportReadModel>
{
    private readonly IMongoDatabase _mongo;

    public GetFinancialReportQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<FinancialReportReadModel>> Handle(
        GetFinancialReportQuery q,
        CancellationToken ct
    )
    {
        var salesCol = _mongo.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);
        var purchaseCol = _mongo.GetCollection<PurchaseOrderReadModel>(
            MongoCollections.PurchaseOrders
        );

        var sales = await salesCol
            .Find(x => x.OrderDate >= q.FromUtc && x.OrderDate <= q.ToUtc)
            .ToListAsync(ct);
        var purchases = await purchaseCol
            .Find(x => x.OrderDate >= q.FromUtc && x.OrderDate <= q.ToUtc)
            .ToListAsync(ct);

        var salesTotal = sales.Sum(x => x.Total);
        var expensesTotal = purchases.Sum(x => x.Total);
        var hasData = sales.Any() || purchases.Any();

        if (!hasData)
        {
            return Result.Success(
                new FinancialReportReadModel
                {
                    FromUtc = q.FromUtc,
                    ToUtc = q.ToUtc,
                    SalesTotal = 0m,
                    ExpensesTotal = 0m,
                    Profit = 0m,
                    HasData = false,
                    Message = "Sin información disponible",
                }
            );
        }

        return Result.Success(
            new FinancialReportReadModel
            {
                FromUtc = q.FromUtc,
                ToUtc = q.ToUtc,
                SalesTotal = salesTotal,
                ExpensesTotal = expensesTotal,
                Profit = salesTotal - expensesTotal,
                HasData = true,
                Message = null,
            }
        );
    }
}
