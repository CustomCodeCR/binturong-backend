using Application.Abstractions.Messaging;
using Application.ReadModels.Accounting;
using Application.ReadModels.Common;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Accounting.GetReconciliationSummary;

internal sealed class GetAccountingReconciliationSummaryQueryHandler
    : IQueryHandler<
        GetAccountingReconciliationSummaryQuery,
        AccountingReconciliationSummaryReadModel
    >
{
    private readonly IMongoDatabase _mongo;

    public GetAccountingReconciliationSummaryQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<AccountingReconciliationSummaryReadModel>> Handle(
        GetAccountingReconciliationSummaryQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<AccountingEntryReadModel>(
            MongoCollections.AccountingEntries
        );
        var docs = await col.Find(Builders<AccountingEntryReadModel>.Filter.Empty).ToListAsync(ct);

        var unmatched = docs.Where(x => !x.IsReconciled)
            .OrderByDescending(x => x.EntryDateUtc)
            .Select(x => new AccountingUnmatchedItemReadModel
            {
                AccountingEntryId = x.AccountingEntryId,
                EntryType = x.EntryType,
                Amount = x.Amount,
                Detail = x.Detail,
                EntryDateUtc = x.EntryDateUtc,
                InvoiceNumber = x.InvoiceNumber,
            })
            .ToArray();

        return Result.Success(
            new AccountingReconciliationSummaryReadModel
            {
                MatchedCount = docs.Count(x => x.IsReconciled),
                UnmatchedCount = docs.Count(x => !x.IsReconciled),
                Differences = unmatched,
            }
        );
    }
}
