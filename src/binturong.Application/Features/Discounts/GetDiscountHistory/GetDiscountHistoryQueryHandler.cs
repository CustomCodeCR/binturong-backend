using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Discounts;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Discounts.GetDiscountHistory;

internal sealed class GetDiscountHistoryQueryHandler
    : IQueryHandler<GetDiscountHistoryQuery, IReadOnlyList<DiscountHistoryReadModel>>
{
    private readonly IMongoDatabase _mongo;

    public GetDiscountHistoryQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<IReadOnlyList<DiscountHistoryReadModel>>> Handle(
        GetDiscountHistoryQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<DiscountHistoryReadModel>(MongoCollections.DiscountHistory);

        var builder = Builders<DiscountHistoryReadModel>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            filter &= builder.Or(
                builder.Regex(
                    x => x.SalesOrderCode,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                builder.Regex(x => x.Reason, new MongoDB.Bson.BsonRegularExpression(s, "i")),
                builder.Regex(x => x.UserName, new MongoDB.Bson.BsonRegularExpression(s, "i"))
            );
        }

        if (q.UserId.HasValue)
            filter &= builder.Eq(x => x.UserId, q.UserId.Value);

        if (q.FromUtc.HasValue)
            filter &= builder.Gte(x => x.EventDateUtc, q.FromUtc.Value);

        if (q.ToUtc.HasValue)
            filter &= builder.Lte(x => x.EventDateUtc, q.ToUtc.Value);

        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.EventDateUtc)
            .Skip(skip)
            .Limit(Math.Clamp(q.PageSize, 1, 500))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<DiscountHistoryReadModel>>(docs);
    }
}
