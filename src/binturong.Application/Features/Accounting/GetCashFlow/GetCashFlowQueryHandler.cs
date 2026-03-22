using Application.Abstractions.Messaging;
using Application.ReadModels.Accounting;
using Application.ReadModels.Common;
using Domain.Accounting;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Accounting.GetCashFlow;

internal sealed class GetCashFlowQueryHandler : IQueryHandler<GetCashFlowQuery, CashFlowReadModel>
{
    private readonly IMongoDatabase _mongo;

    public GetCashFlowQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<CashFlowReadModel>> Handle(GetCashFlowQuery q, CancellationToken ct)
    {
        if (q.FromUtc > q.ToUtc)
            return Result.Failure<CashFlowReadModel>(AccountingErrors.DateRangeInvalid);

        var col = _mongo.GetCollection<AccountingEntryReadModel>(
            MongoCollections.AccountingEntries
        );
        var docs = await col.Find(x => x.EntryDateUtc >= q.FromUtc && x.EntryDateUtc <= q.ToUtc)
            .SortBy(x => x.EntryDateUtc)
            .ToListAsync(ct);

        decimal runningBalance = 0m;
        var grouped = docs.GroupBy(x => x.EntryDateUtc.Date).OrderBy(x => x.Key).ToList();

        var points = new List<CashFlowPointReadModel>();

        foreach (var day in grouped)
        {
            var income = day.Where(x => x.EntryType == "Income").Sum(x => x.Amount);
            var expense = day.Where(x => x.EntryType == "Expense").Sum(x => x.Amount);
            runningBalance += income - expense;

            points.Add(
                new CashFlowPointReadModel
                {
                    DateUtc = DateTime.SpecifyKind(day.Key, DateTimeKind.Utc),
                    Income = income,
                    Expense = expense,
                    Balance = runningBalance,
                }
            );
        }

        var totalIncome = docs.Where(x => x.EntryType == "Income").Sum(x => x.Amount);
        var totalExpenses = docs.Where(x => x.EntryType == "Expense").Sum(x => x.Amount);

        return Result.Success(
            new CashFlowReadModel
            {
                FromUtc = q.FromUtc,
                ToUtc = q.ToUtc,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                Balance = totalIncome - totalExpenses,
                Points = points,
            }
        );
    }
}
