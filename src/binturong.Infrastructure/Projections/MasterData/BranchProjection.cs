using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using Domain.Branches;
using MongoDB.Driver;

namespace Infrastructure.Projections.MasterData;

internal sealed class BranchProjection
    : IProjector<BranchCreatedDomainEvent>,
        IProjector<BranchUpdatedDomainEvent>,
        IProjector<BranchDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public BranchProjection(IMongoDatabase db) => _db = db;

    public Task ProjectAsync(BranchCreatedDomainEvent e, CancellationToken ct) =>
        UpsertAsync(
            branchId: e.BranchId,
            code: e.Code,
            name: e.Name,
            address: e.Address,
            phone: e.Phone,
            isActive: e.IsActive,
            createdAt: e.CreatedAt,
            updatedAt: e.UpdatedAt,
            ct
        );

    public Task ProjectAsync(BranchUpdatedDomainEvent e, CancellationToken ct) =>
        UpsertAsync(
            branchId: e.BranchId,
            code: e.Code,
            name: e.Name,
            address: e.Address,
            phone: e.Phone,
            isActive: e.IsActive,
            createdAt: null, // no tocar createdAt en updates
            updatedAt: e.UpdatedAt,
            ct
        );

    public async Task ProjectAsync(BranchDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);

        var id = $"branch:{e.BranchId}";
        await col.DeleteOneAsync(x => x.Id == id, ct);
    }

    private async Task UpsertAsync(
        Guid branchId,
        string code,
        string name,
        string address,
        string phone,
        bool isActive,
        DateTime? createdAt,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);

        var id = $"branch:{branchId}";
        var filter = Builders<BranchReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<BranchReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.BranchId, branchId)
            .Set(x => x.Code, code)
            .Set(x => x.Name, name)
            .Set(x => x.Address, address)
            .Set(x => x.Phone, phone)
            .Set(x => x.IsActive, isActive)
            .Set(x => x.UpdatedAt, updatedAt);

        if (createdAt is not null)
        {
            update = update.SetOnInsert(x => x.CreatedAt, createdAt.Value);
        }

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}
