using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Discounts;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Discounts.GetPolicies;

internal sealed class GetDiscountPoliciesQueryHandler
    : IQueryHandler<GetDiscountPoliciesQuery, IReadOnlyList<DiscountPolicyReadModel>>
{
    private readonly IMongoDatabase _mongo;

    public GetDiscountPoliciesQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<IReadOnlyList<DiscountPolicyReadModel>>> Handle(
        GetDiscountPoliciesQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<DiscountPolicyReadModel>(MongoCollections.DiscountPolicies);
        var builder = Builders<DiscountPolicyReadModel>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            filter = builder.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(s, "i"));
        }

        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);

        var docs = await col.Find(filter)
            .SortBy(x => x.Name)
            .Skip(skip)
            .Limit(Math.Clamp(q.PageSize, 1, 200))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<DiscountPolicyReadModel>>(docs);
    }
}
