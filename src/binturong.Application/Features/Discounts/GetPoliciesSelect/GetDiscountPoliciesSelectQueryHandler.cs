using System.Text.RegularExpressions;
using Application.Abstractions.Messaging;
using Application.Common.Selects;
using Application.ReadModels.Common;
using Application.ReadModels.Discounts;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Discounts.GetPoliciesSelect;

internal sealed class GetDiscountPoliciesSelectQueryHandler
    : IQueryHandler<GetDiscountPoliciesSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private readonly IMongoDatabase _db;

    public GetDiscountPoliciesSelectQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<SelectOptionDto>>> Handle(
        GetDiscountPoliciesSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<DiscountPolicyReadModel>(MongoCollections.DiscountPolicies);

        var filter = BuildFilter(query.Search);

        var docs = await col.Find(filter).SortBy(x => x.Name).Limit(100).ToListAsync(ct);

        var result = docs.Select(x => new SelectOptionDto(x.PolicyId.ToString(), x.Name, x.Name))
            .ToList();

        return Result.Success<IReadOnlyList<SelectOptionDto>>(result);
    }

    private static FilterDefinition<DiscountPolicyReadModel> BuildFilter(string? search)
    {
        var builder = Builders<DiscountPolicyReadModel>.Filter;
        var filter = builder.Eq(x => x.IsActive, true);

        if (string.IsNullOrWhiteSpace(search))
            return filter;

        var escaped = Regex.Escape(search.Trim());
        var containsRegex = new BsonRegularExpression(escaped, "i");

        return builder.And(filter, builder.Regex(x => x.Name, containsRegex));
    }
}
