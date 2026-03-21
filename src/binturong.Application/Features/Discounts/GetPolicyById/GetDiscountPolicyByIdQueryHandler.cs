using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Discounts;
using Domain.Discounts;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Discounts.GetPolicyById;

internal sealed class GetDiscountPolicyByIdQueryHandler
    : IQueryHandler<GetDiscountPolicyByIdQuery, DiscountPolicyReadModel>
{
    private readonly IMongoDatabase _mongo;

    public GetDiscountPolicyByIdQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<DiscountPolicyReadModel>> Handle(
        GetDiscountPolicyByIdQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<DiscountPolicyReadModel>(MongoCollections.DiscountPolicies);
        var doc = await col.Find(x => x.PolicyId == q.PolicyId).FirstOrDefaultAsync(ct);

        return doc is null
            ? Result.Failure<DiscountPolicyReadModel>(DiscountErrors.PolicyNotFound(q.PolicyId))
            : Result.Success(doc);
    }
}
