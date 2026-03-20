using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using Application.ReadModels.MasterData;
using Application.ReadModels.Services;
using Domain.ServiceOrders;
using MongoDB.Driver;

namespace Infrastructure.Projections.Services;

internal sealed class ServiceOrderProjection
    : IProjector<ServiceOrderCreatedDomainEvent>,
        IProjector<ServiceOrderUpdatedDomainEvent>,
        IProjector<ServiceOrderServiceAddedDomainEvent>,
        IProjector<ServiceOrderTechnicianAssignedDomainEvent>,
        IProjector<ServiceOrderMaterialAddedDomainEvent>,
        IProjector<ServiceOrderChecklistAddedDomainEvent>,
        IProjector<ServiceOrderPhotoAddedDomainEvent>,
        IProjector<ServiceOrderClosedDomainEvent>,
        IProjector<ServiceOrderDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public ServiceOrderProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(ServiceOrderCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);
        var clientsCol = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);

        var branchesCol = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);
        var contractsCol = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);

        var client = await clientsCol.Find(x => x.ClientId == e.ClientId).FirstOrDefaultAsync(ct);

        var branch = e.BranchId.HasValue
            ? await branchesCol.Find(x => x.BranchId == e.BranchId.Value).FirstOrDefaultAsync(ct)
            : null;

        var contract = e.ContractId.HasValue
            ? await contractsCol
                .Find(x => x.ContractId == e.ContractId.Value)
                .FirstOrDefaultAsync(ct)
            : null;

        var id = $"service_order:{e.ServiceOrderId}";
        var filter = Builders<ServiceOrderReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ServiceOrderReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ServiceOrderId, e.ServiceOrderId)
            .Set(x => x.Code, e.Code)
            .Set(x => x.ClientId, e.ClientId)
            .Set(x => x.ClientName, client?.TradeName ?? client?.ContactName ?? string.Empty)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.BranchName, branch?.Name)
            .Set(x => x.ContractId, e.ContractId)
            .Set(x => x.ContractCode, contract?.Code)
            .Set(x => x.ScheduledDate, e.ScheduledDateUtc)
            .Set(x => x.ClosedDate, null)
            .Set(x => x.Status, e.Status)
            .Set(x => x.ServiceAddress, e.ServiceAddress)
            .Set(x => x.Notes, string.IsNullOrWhiteSpace(e.Notes) ? null : e.Notes);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(ServiceOrderUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);

        var filter = Builders<ServiceOrderReadModel>.Filter.Eq(
            x => x.Id,
            $"service_order:{e.ServiceOrderId}"
        );

        var update = Builders<ServiceOrderReadModel>
            .Update.Set(x => x.ScheduledDate, e.ScheduledDateUtc)
            .Set(x => x.Status, e.Status)
            .Set(x => x.ServiceAddress, e.ServiceAddress)
            .Set(x => x.Notes, string.IsNullOrWhiteSpace(e.Notes) ? null : e.Notes);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(ServiceOrderServiceAddedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);

        var filter = Builders<ServiceOrderReadModel>.Filter.Eq(
            x => x.Id,
            $"service_order:{e.ServiceOrderId}"
        );

        var line = new ServiceOrderServiceLineReadModel
        {
            ServiceOrderServiceId = e.ServiceOrderServiceId,
            ServiceId = e.ServiceId,
            ServiceName = e.ServiceName,
            Quantity = e.Quantity,
            RateApplied = e.RateApplied,
            LineTotal = e.LineTotal,
        };

        var update = Builders<ServiceOrderReadModel>.Update.Push(x => x.Services, line);
        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(
        ServiceOrderTechnicianAssignedDomainEvent e,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);

        var filter = Builders<ServiceOrderReadModel>.Filter.Eq(
            x => x.Id,
            $"service_order:{e.ServiceOrderId}"
        );

        var line = new ServiceOrderTechnicianLineReadModel
        {
            ServiceOrderTechnicianId = e.ServiceOrderTechnicianId,
            EmployeeId = e.EmployeeId,
            EmployeeName = e.EmployeeName,
            TechRole = e.TechRole,
            AssignedAtUtc = e.AssignedAtUtc,
        };

        var update = Builders<ServiceOrderReadModel>.Update.Push(x => x.Technicians, line);
        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(ServiceOrderMaterialAddedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);

        var filter = Builders<ServiceOrderReadModel>.Filter.Eq(
            x => x.Id,
            $"service_order:{e.ServiceOrderId}"
        );

        var line = new ServiceOrderMaterialLineReadModel
        {
            ServiceOrderMaterialId = e.ServiceOrderMaterialId,
            ProductId = e.ProductId,
            ProductName = e.ProductName,
            Quantity = e.Quantity,
            EstimatedCost = e.EstimatedCost,
        };

        var update = Builders<ServiceOrderReadModel>.Update.Push(x => x.Materials, line);
        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(ServiceOrderChecklistAddedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);

        var filter = Builders<ServiceOrderReadModel>.Filter.Eq(
            x => x.Id,
            $"service_order:{e.ServiceOrderId}"
        );

        var line = new ServiceOrderChecklistLineReadModel
        {
            ChecklistId = e.ChecklistId,
            Description = e.Description,
            IsCompleted = e.IsCompleted,
        };

        var update = Builders<ServiceOrderReadModel>.Update.Push(x => x.Checklists, line);
        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(ServiceOrderPhotoAddedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);

        var filter = Builders<ServiceOrderReadModel>.Filter.Eq(
            x => x.Id,
            $"service_order:{e.ServiceOrderId}"
        );

        var line = new ServiceOrderPhotoLineReadModel
        {
            PhotoId = e.PhotoId,
            PhotoS3Key = e.PhotoS3Key,
            Description = string.IsNullOrWhiteSpace(e.Description) ? null : e.Description,
        };

        var update = Builders<ServiceOrderReadModel>.Update.Push(x => x.Photos, line);
        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(ServiceOrderClosedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);

        var filter = Builders<ServiceOrderReadModel>.Filter.Eq(
            x => x.Id,
            $"service_order:{e.ServiceOrderId}"
        );

        var update = Builders<ServiceOrderReadModel>
            .Update.Set(x => x.ClosedDate, e.ClosedDateUtc)
            .Set(x => x.Status, "Closed");

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(ServiceOrderDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);
        await col.DeleteOneAsync(x => x.Id == $"service_order:{e.ServiceOrderId}", ct);
    }
}
