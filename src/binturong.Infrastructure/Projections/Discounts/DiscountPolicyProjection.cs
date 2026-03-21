using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Discounts;
using Domain.Discounts;
using MongoDB.Driver;

namespace Infrastructure.Projections.Discounts;

internal sealed class DiscountPolicyProjection
    : IProjector<DiscountPolicyCreatedDomainEvent>,
        IProjector<DiscountPolicyUpdatedDomainEvent>,
        IProjector<DiscountPolicyDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public DiscountPolicyProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(DiscountPolicyCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<DiscountPolicyReadModel>(MongoCollections.DiscountPolicies);

        var id = $"discount_policy:{e.PolicyId}";
        var filter = Builders<DiscountPolicyReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<DiscountPolicyReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.PolicyId, e.PolicyId)
            .Set(x => x.Name, e.Name)
            .Set(x => x.MaxDiscountPercentage, e.MaxDiscountPercentage)
            .Set(x => x.RequiresApprovalAboveLimit, e.RequiresApprovalAboveLimit)
            .Set(x => x.IsActive, e.IsActive);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(DiscountPolicyUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<DiscountPolicyReadModel>(MongoCollections.DiscountPolicies);

        var id = $"discount_policy:{e.PolicyId}";
        var filter = Builders<DiscountPolicyReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<DiscountPolicyReadModel>
            .Update.Set(x => x.Name, e.Name)
            .Set(x => x.MaxDiscountPercentage, e.MaxDiscountPercentage)
            .Set(x => x.RequiresApprovalAboveLimit, e.RequiresApprovalAboveLimit)
            .Set(x => x.IsActive, e.IsActive);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(DiscountPolicyDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<DiscountPolicyReadModel>(MongoCollections.DiscountPolicies);
        await col.DeleteOneAsync(x => x.Id == $"discount_policy:{e.PolicyId}", ct);
    }
}
