using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using Domain.ContractBillingMilestones;
using Domain.Contracts;
using MongoDB.Driver;

namespace Infrastructure.Projections.Sales;

internal sealed class ContractProjection
    : IProjector<ContractCreatedDomainEvent>,
        IProjector<ContractUpdatedDomainEvent>,
        IProjector<ContractDeletedDomainEvent>,
        IProjector<ContractMilestoneAddedDomainEvent>,
        IProjector<ContractMilestoneUpdatedDomainEvent>,
        IProjector<ContractMilestoneRemovedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public ContractProjection(IMongoDatabase db) => _db = db;

    public Task ProjectAsync(ContractCreatedDomainEvent e, CancellationToken ct) =>
        UpsertHeaderAsync(
            e.ContractId,
            e.Code,
            e.ClientId,
            e.QuoteId,
            e.SalesOrderId,
            e.StartDate,
            e.EndDate,
            e.Status,
            e.Description,
            e.Notes,
            ct
        );

    public Task ProjectAsync(ContractUpdatedDomainEvent e, CancellationToken ct) =>
        UpsertHeaderAsync(
            e.ContractId,
            e.Code,
            e.ClientId,
            e.QuoteId,
            e.SalesOrderId,
            e.StartDate,
            e.EndDate,
            e.Status,
            e.Description,
            e.Notes,
            ct
        );

    public async Task ProjectAsync(ContractDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);
        var id = $"contract:{e.ContractId}";
        await col.DeleteOneAsync(x => x.Id == id, ct);
    }

    public Task ProjectAsync(ContractMilestoneAddedDomainEvent e, CancellationToken ct) =>
        UpsertMilestoneCoreAsync(
            e.ContractId,
            e.MilestoneId,
            e.Description,
            e.Percentage,
            e.Amount,
            e.ScheduledDate,
            e.IsBilled,
            e.InvoiceId,
            ct
        );

    public Task ProjectAsync(ContractMilestoneUpdatedDomainEvent e, CancellationToken ct) =>
        UpsertMilestoneCoreAsync(
            e.ContractId,
            e.MilestoneId,
            e.Description,
            e.Percentage,
            e.Amount,
            e.ScheduledDate,
            e.IsBilled,
            e.InvoiceId,
            ct
        );

    public async Task ProjectAsync(ContractMilestoneRemovedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);
        var id = $"contract:{e.ContractId}";
        var filter = Builders<ContractReadModel>.Filter.Eq(x => x.Id, id);

        var pull = Builders<ContractReadModel>.Update.PullFilter(
            x => x.Milestones,
            m => m.MilestoneId == e.MilestoneId
        );

        await col.UpdateOneAsync(filter, pull, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task UpsertHeaderAsync(
        Guid contractId,
        string code,
        Guid clientId,
        Guid? quoteId,
        Guid? salesOrderId,
        DateOnly startDate,
        DateOnly? endDate,
        string status,
        string? description,
        string? notes,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);

        var id = $"contract:{contractId}";
        var filter = Builders<ContractReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ContractReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ContractId, contractId)
            .Set(x => x.Code, code)
            .Set(x => x.ClientId, clientId)
            .Set(x => x.ClientName, string.Empty) // fill later if you project client snapshots
            .Set(x => x.QuoteId, quoteId)
            .Set(x => x.SalesOrderId, salesOrderId)
            .Set(x => x.StartDate, ToUtcDate(startDate))
            .Set(x => x.EndDate, endDate is null ? null : ToUtcDate(endDate.Value))
            .Set(x => x.Status, status)
            .Set(x => x.Description, string.IsNullOrWhiteSpace(description) ? null : description)
            .Set(x => x.Notes, string.IsNullOrWhiteSpace(notes) ? null : notes);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task UpsertMilestoneCoreAsync(
        Guid contractId,
        Guid milestoneId,
        string description,
        decimal percentage,
        decimal amount,
        DateOnly scheduledDate,
        bool isBilled,
        Guid? invoiceId,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);

        var id = $"contract:{contractId}";
        var filter = Builders<ContractReadModel>.Filter.Eq(x => x.Id, id);

        // Remove any existing milestone with same id
        var pull = Builders<ContractReadModel>.Update.PullFilter(
            x => x.Milestones,
            m => m.MilestoneId == milestoneId
        );
        await col.UpdateOneAsync(filter, pull, new UpdateOptions { IsUpsert = true }, ct);

        // Push the new milestone
        var line = new ContractMilestoneReadModel
        {
            MilestoneId = milestoneId,
            Description = description,
            Percentage = percentage,
            Amount = amount,
            ScheduledDate = ToUtcDate(scheduledDate),
            IsBilled = isBilled,
            InvoiceId = invoiceId,
        };

        var push = Builders<ContractReadModel>.Update.Push(x => x.Milestones, line);
        await col.UpdateOneAsync(filter, push, new UpdateOptions { IsUpsert = true }, ct);
    }

    private static DateTime ToUtcDate(DateOnly d) =>
        new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
}
