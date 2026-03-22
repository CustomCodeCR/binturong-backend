using Application.Abstractions.Messaging;
using Application.ReadModels.Accounting;
using Application.ReadModels.Common;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Accounting.GetEntries;

internal sealed class GetAccountingEntriesQueryHandler
    : IQueryHandler<GetAccountingEntriesQuery, IReadOnlyList<AccountingEntryReadModel>>
{
    private readonly IMongoDatabase _mongo;

    public GetAccountingEntriesQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<IReadOnlyList<AccountingEntryReadModel>>> Handle(
        GetAccountingEntriesQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<AccountingEntryReadModel>(
            MongoCollections.AccountingEntries
        );

        var builder = Builders<AccountingEntryReadModel>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            filter &= builder.Or(
                builder.Regex(x => x.Detail, new MongoDB.Bson.BsonRegularExpression(s, "i")),
                builder.Regex(x => x.Category, new MongoDB.Bson.BsonRegularExpression(s, "i")),
                builder.Regex(x => x.InvoiceNumber, new MongoDB.Bson.BsonRegularExpression(s, "i"))
            );
        }

        if (!string.IsNullOrWhiteSpace(q.EntryType))
            filter &= builder.Eq(x => x.EntryType, q.EntryType.Trim());

        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.EntryDateUtc)
            .Skip(skip)
            .Limit(Math.Clamp(q.PageSize, 1, 200))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<AccountingEntryReadModel>>(docs);
    }
}
