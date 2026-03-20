using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Services;
using Domain.Services;
using MongoDB.Driver;

namespace Infrastructure.Projections.Services;

internal sealed class ServiceProjection
    : IProjector<ServiceCreatedDomainEvent>,
        IProjector<ServiceUpdatedDomainEvent>,
        IProjector<ServiceDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public ServiceProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(ServiceCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ServiceReadModel>(MongoCollections.Services);

        var id = $"service:{e.ServiceId}";
        var filter = Builders<ServiceReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ServiceReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ServiceId, e.ServiceId)
            .Set(x => x.Code, e.Code)
            .Set(x => x.Name, e.Name)
            .Set(
                x => x.Description,
                string.IsNullOrWhiteSpace(e.Description) ? null : e.Description
            )
            .Set(x => x.CategoryId, e.CategoryId)
            .Set(x => x.CategoryName, e.CategoryName)
            .Set(x => x.IsCategoryProtected, e.IsCategoryProtected)
            .Set(x => x.StandardTimeMin, e.StandardTimeMin)
            .Set(x => x.BaseRate, e.BaseRate)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.AvailabilityStatus, e.AvailabilityStatus);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(ServiceUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ServiceReadModel>(MongoCollections.Services);

        var id = $"service:{e.ServiceId}";
        var filter = Builders<ServiceReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ServiceReadModel>
            .Update.Set(x => x.Code, e.Code)
            .Set(x => x.Name, e.Name)
            .Set(
                x => x.Description,
                string.IsNullOrWhiteSpace(e.Description) ? null : e.Description
            )
            .Set(x => x.CategoryId, e.CategoryId)
            .Set(x => x.CategoryName, e.CategoryName)
            .Set(x => x.IsCategoryProtected, e.IsCategoryProtected)
            .Set(x => x.StandardTimeMin, e.StandardTimeMin)
            .Set(x => x.BaseRate, e.BaseRate)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.AvailabilityStatus, e.AvailabilityStatus);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(ServiceDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ServiceReadModel>(MongoCollections.Services);
        await col.DeleteOneAsync(x => x.Id == $"service:{e.ServiceId}", ct);
    }
}
