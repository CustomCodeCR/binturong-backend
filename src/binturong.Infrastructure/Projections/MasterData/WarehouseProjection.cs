using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using Domain.Warehouses;
using MongoDB.Driver;

namespace Infrastructure.Projections.MasterData;

internal sealed class WarehouseProjection
    : IProjector<WarehouseCreatedDomainEvent>,
        IProjector<WarehouseUpdatedDomainEvent>,
        IProjector<WarehouseDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public WarehouseProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(WarehouseCreatedDomainEvent e, CancellationToken ct)
    {
        await UpsertWarehouseAsync(e, createdAt: e.CreatedAt, updatedAt: e.UpdatedAt, ct);
        await UpsertBranchSummaryAsync(
            e.BranchId,
            e.WarehouseId,
            e.Code,
            e.Name,
            e.IsActive,
            e.UpdatedAt,
            ct
        );
    }

    public async Task ProjectAsync(WarehouseUpdatedDomainEvent e, CancellationToken ct)
    {
        await UpsertWarehouseAsync(e, createdAt: null, updatedAt: e.UpdatedAt, ct);
        await UpsertBranchSummaryAsync(
            e.BranchId,
            e.WarehouseId,
            e.Code,
            e.Name,
            e.IsActive,
            e.UpdatedAt,
            ct
        );
    }

    public async Task ProjectAsync(WarehouseDeletedDomainEvent e, CancellationToken ct)
    {
        var warehouses = _db.GetCollection<WarehouseReadModel>(MongoCollections.Warehouses);
        await warehouses.DeleteOneAsync(x => x.Id == $"warehouse:{e.WarehouseId}", ct);

        var branches = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);
        var branchId = $"branch:{e.BranchId}";

        var update = Builders<BranchReadModel>
            .Update.PullFilter(x => x.Warehouses, w => w.WarehouseId == e.WarehouseId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await branches.UpdateOneAsync(x => x.Id == branchId, update, cancellationToken: ct);
    }

    private async Task UpsertWarehouseAsync(
        WarehouseCreatedDomainEvent e,
        DateTime? createdAt,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<WarehouseReadModel>(MongoCollections.Warehouses);

        var id = $"warehouse:{e.WarehouseId}";
        var filter = Builders<WarehouseReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<WarehouseReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.WarehouseId, e.WarehouseId)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.BranchCode, e.BranchCode)
            .Set(x => x.BranchName, e.BranchName)
            .Set(x => x.Code, e.Code)
            .Set(x => x.Name, e.Name)
            .Set(x => x.Description, e.Description)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.UpdatedAt, updatedAt);

        if (createdAt is not null)
            update = update.SetOnInsert(x => x.CreatedAt, createdAt.Value);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task UpsertWarehouseAsync(
        WarehouseUpdatedDomainEvent e,
        DateTime? createdAt,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<WarehouseReadModel>(MongoCollections.Warehouses);

        var id = $"warehouse:{e.WarehouseId}";
        var filter = Builders<WarehouseReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<WarehouseReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.WarehouseId, e.WarehouseId)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.BranchCode, e.BranchCode)
            .Set(x => x.BranchName, e.BranchName)
            .Set(x => x.Code, e.Code)
            .Set(x => x.Name, e.Name)
            .Set(x => x.Description, e.Description)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.UpdatedAt, updatedAt);

        if (createdAt is not null)
            update = update.SetOnInsert(x => x.CreatedAt, createdAt.Value);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task UpsertBranchSummaryAsync(
        Guid branchId,
        Guid warehouseId,
        string code,
        string name,
        bool isActive,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var branches = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);
        var id = $"branch:{branchId}";

        // idempotent
        var pull = Builders<BranchReadModel>.Update.PullFilter(
            x => x.Warehouses,
            w => w.WarehouseId == warehouseId
        );
        await branches.UpdateOneAsync(x => x.Id == id, pull, cancellationToken: ct);

        var summary = new BranchWarehouseSummaryReadModel
        {
            WarehouseId = warehouseId,
            Code = code,
            Name = name,
            IsActive = isActive,
        };

        var push = Builders<BranchReadModel>
            .Update.Push(x => x.Warehouses, summary)
            .Set(x => x.UpdatedAt, updatedAt);

        await branches.UpdateOneAsync(x => x.Id == id, push, cancellationToken: ct);
    }
}
