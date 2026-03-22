using Application.Abstractions.Messaging;
using Application.ReadModels.Accounting;
using Application.ReadModels.Common;
using Domain.Accounting;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Accounting.GetIncomeStatement;

internal sealed class GetIncomeStatementQueryHandler
    : IQueryHandler<GetIncomeStatementQuery, IncomeStatementReadModel>
{
    private readonly IMongoDatabase _mongo;

    public GetIncomeStatementQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<IncomeStatementReadModel>> Handle(
        GetIncomeStatementQuery q,
        CancellationToken ct
    )
    {
        if (q.FromUtc > q.ToUtc)
            return Result.Failure<IncomeStatementReadModel>(AccountingErrors.DateRangeInvalid);

        var col = _mongo.GetCollection<AccountingEntryReadModel>(
            MongoCollections.AccountingEntries
        );
        var docs = await col.Find(x => x.EntryDateUtc >= q.FromUtc && x.EntryDateUtc <= q.ToUtc)
            .ToListAsync(ct);

        if (docs.Count == 0)
        {
            return Result.Success(
                new IncomeStatementReadModel
                {
                    FromUtc = q.FromUtc,
                    ToUtc = q.ToUtc,
                    TotalIncome = 0m,
                    TotalExpenses = 0m,
                    GrossProfit = 0m,
                    NetProfit = 0m,
                    HasData = false,
                    Message = "No hay movimientos registrados",
                }
            );
        }

        var totalIncome = docs.Where(x => x.EntryType == "Income").Sum(x => x.Amount);
        var totalExpenses = docs.Where(x => x.EntryType == "Expense").Sum(x => x.Amount);

        return Result.Success(
            new IncomeStatementReadModel
            {
                FromUtc = q.FromUtc,
                ToUtc = q.ToUtc,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                GrossProfit = totalIncome - totalExpenses,
                NetProfit = totalIncome - totalExpenses,
                HasData = true,
                Message = null,
            }
        );
    }
}
