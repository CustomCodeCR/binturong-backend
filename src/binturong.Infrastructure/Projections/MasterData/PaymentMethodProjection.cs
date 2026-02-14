using Application.Abstractions.Projections;
using Application.ReadModels.MasterData;
using Domain.PaymentMethods;
using MongoDB.Driver;

namespace Infrastructure.Projections.MasterData;

internal sealed class PaymentMethodProjection
    : IProjector<PaymentMethodCreatedDomainEvent>,
        IProjector<PaymentMethodUpdatedDomainEvent>,
        IProjector<PaymentMethodDeletedDomainEvent>
{
    private const string CollectionName = "payment_methods";
    private readonly IMongoDatabase _db;

    public PaymentMethodProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(PaymentMethodCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PaymentMethodReadModel>(CollectionName);

        var id = $"pm:{e.PaymentMethodId}";
        var filter = Builders<PaymentMethodReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<PaymentMethodReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.PaymentMethodId, e.PaymentMethodId)
            .Set(x => x.Code, e.Code)
            .Set(x => x.Description, e.Description)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.UpdatedAt, e.AtUtc);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(PaymentMethodUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PaymentMethodReadModel>(CollectionName);

        var id = $"pm:{e.PaymentMethodId}";
        var filter = Builders<PaymentMethodReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<PaymentMethodReadModel>
            .Update.Set(x => x.Code, e.Code)
            .Set(x => x.Description, e.Description)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.UpdatedAt, e.AtUtc);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(PaymentMethodDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PaymentMethodReadModel>(CollectionName);
        await col.DeleteOneAsync(x => x.Id == $"pm:{e.PaymentMethodId}", ct);
    }
}
