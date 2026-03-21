using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Discounts;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Discounts.GetApprovalRequests;

internal sealed class GetDiscountApprovalRequestsQueryHandler
    : IQueryHandler<
        GetDiscountApprovalRequestsQuery,
        IReadOnlyList<DiscountApprovalRequestReadModel>
    >
{
    private readonly IMongoDatabase _mongo;

    public GetDiscountApprovalRequestsQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<IReadOnlyList<DiscountApprovalRequestReadModel>>> Handle(
        GetDiscountApprovalRequestsQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<DiscountApprovalRequestReadModel>(
            MongoCollections.DiscountApprovalRequests
        );

        var builder = Builders<DiscountApprovalRequestReadModel>.Filter;
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
                builder.Regex(
                    x => x.RequestedByUserName,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(q.Status))
            filter &= builder.Eq(x => x.Status, q.Status.Trim());

        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.RequestedAtUtc)
            .Skip(skip)
            .Limit(Math.Clamp(q.PageSize, 1, 200))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<DiscountApprovalRequestReadModel>>(docs);
    }
}
