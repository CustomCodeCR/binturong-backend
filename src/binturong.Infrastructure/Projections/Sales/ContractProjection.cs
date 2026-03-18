using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
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
        IProjector<ContractMilestoneRemovedDomainEvent>,
        IProjector<ContractRenewedDomainEvent>,
        IProjector<ContractExpiryNoticeSentDomainEvent>,
        IProjector<ContractCreatedFromQuoteDomainEvent>,
        IProjector<ContractAttachmentUploadedDomainEvent>,
        IProjector<ContractAttachmentDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public ContractProjection(IMongoDatabase db)
    {
        _db = db;
    }

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
            e.ResponsibleUserId,
            e.AutoRenewEnabled,
            e.AutoRenewEveryDays,
            e.ExpiryNoticeDays,
            e.ExpiryAlertActive,
            e.ExpiryLastNotifiedAtUtc,
            e.RenewedAtUtc,
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
            e.ResponsibleUserId,
            e.AutoRenewEnabled,
            e.AutoRenewEveryDays,
            e.ExpiryNoticeDays,
            e.ExpiryAlertActive,
            e.ExpiryLastNotifiedAtUtc,
            e.RenewedAtUtc,
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

        await col.UpdateOneAsync(filter, pull, cancellationToken: ct);
    }

    public async Task ProjectAsync(ContractRenewedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);
        var id = $"contract:{e.ContractId}";
        var filter = Builders<ContractReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ContractReadModel>
            .Update.Set(x => x.StartDate, ToUtcDate(e.NewStartDate))
            .Set(x => x.EndDate, ToUtcDate(e.NewEndDate))
            .Set(x => x.Status, "Active")
            .Set(x => x.RenewedAtUtc, e.RenewedAtUtc)
            .Set(x => x.ExpiryAlertActive, false);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false }, ct);
    }

    public async Task ProjectAsync(ContractExpiryNoticeSentDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);
        var id = $"contract:{e.ContractId}";
        var filter = Builders<ContractReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ContractReadModel>
            .Update.Set(x => x.Status, "ExpiringSoon")
            .Set(x => x.ExpiryAlertActive, true)
            .Set(x => x.ExpiryLastNotifiedAtUtc, e.NotifiedAtUtc)
            .Set(x => x.ExpiryNoticeDays, e.NoticeDays)
            .Set(x => x.EndDate, e.EndDate is null ? null : ToUtcDate(e.EndDate.Value));

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false }, ct);
    }

    public async Task ProjectAsync(ContractCreatedFromQuoteDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);
        var id = $"contract:{e.ContractId}";
        var filter = Builders<ContractReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ContractReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ContractId, e.ContractId)
            .SetOnInsert(x => x.Code, string.Empty)
            .SetOnInsert(x => x.ClientId, Guid.Empty)
            .SetOnInsert(x => x.ClientName, string.Empty)
            .SetOnInsert(x => x.SalesOrderId, null)
            .SetOnInsert(x => x.Description, null)
            .SetOnInsert(x => x.Notes, null)
            .SetOnInsert(x => x.Milestones, new List<ContractMilestoneReadModel>())
            .SetOnInsert(x => x.Attachments, new List<ContractAttachmentReadModel>())
            .Set(x => x.QuoteId, e.QuoteId)
            .Set(x => x.StartDate, ToUtcDate(e.StartDate))
            .Set(x => x.EndDate, e.EndDate is null ? null : ToUtcDate(e.EndDate.Value))
            .Set(x => x.Status, "Active")
            .Set(x => x.ResponsibleUserId, e.ResponsibleUserId)
            .Set(x => x.AutoRenewEnabled, e.AutoRenewEnabled)
            .Set(x => x.AutoRenewEveryDays, e.AutoRenewEveryDays)
            .Set(x => x.ExpiryNoticeDays, e.ExpiryNoticeDays)
            .Set(x => x.ExpiryAlertActive, false)
            .Set(x => x.ExpiryLastNotifiedAtUtc, null)
            .Set(x => x.RenewedAtUtc, null);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public Task ProjectAsync(ContractAttachmentUploadedDomainEvent e, CancellationToken ct) =>
        UpsertAttachmentAsync(
            e.ContractId,
            e.AttachmentId,
            e.FileName,
            e.ContentType,
            e.SizeBytes,
            e.StoragePath,
            e.UploadedAtUtc,
            ct
        );

    public async Task ProjectAsync(ContractAttachmentDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);
        var id = $"contract:{e.ContractId}";
        var filter = Builders<ContractReadModel>.Filter.Eq(x => x.Id, id);

        var pull = Builders<ContractReadModel>.Update.PullFilter(
            x => x.Attachments,
            a => a.AttachmentId == e.AttachmentId
        );

        await col.UpdateOneAsync(filter, pull, cancellationToken: ct);
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
        Guid? responsibleUserId,
        bool autoRenewEnabled,
        int autoRenewEveryDays,
        int expiryNoticeDays,
        bool expiryAlertActive,
        DateTime? expiryLastNotifiedAtUtc,
        DateTime? renewedAtUtc,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);
        var id = $"contract:{contractId}";
        var filter = Builders<ContractReadModel>.Filter.Eq(x => x.Id, id);

        var clientName = await ResolveClientNameAsync(clientId, ct);

        var update = Builders<ContractReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.Milestones, new List<ContractMilestoneReadModel>())
            .SetOnInsert(x => x.Attachments, new List<ContractAttachmentReadModel>())
            .Set(x => x.ContractId, contractId)
            .Set(x => x.Code, code)
            .Set(x => x.ClientId, clientId)
            .Set(x => x.ClientName, clientName)
            .Set(x => x.QuoteId, quoteId)
            .Set(x => x.SalesOrderId, salesOrderId)
            .Set(x => x.StartDate, ToUtcDate(startDate))
            .Set(x => x.EndDate, endDate is null ? null : ToUtcDate(endDate.Value))
            .Set(x => x.Status, status)
            .Set(
                x => x.Description,
                string.IsNullOrWhiteSpace(description) ? null : description.Trim()
            )
            .Set(x => x.Notes, string.IsNullOrWhiteSpace(notes) ? null : notes.Trim())
            .Set(x => x.ResponsibleUserId, responsibleUserId)
            .Set(x => x.AutoRenewEnabled, autoRenewEnabled)
            .Set(x => x.AutoRenewEveryDays, autoRenewEveryDays)
            .Set(x => x.ExpiryNoticeDays, expiryNoticeDays)
            .Set(x => x.ExpiryAlertActive, expiryAlertActive)
            .Set(x => x.ExpiryLastNotifiedAtUtc, expiryLastNotifiedAtUtc)
            .Set(x => x.RenewedAtUtc, renewedAtUtc);

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

        var ensureDocument = Builders<ContractReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ContractId, contractId)
            .SetOnInsert(x => x.Code, string.Empty)
            .SetOnInsert(x => x.ClientId, Guid.Empty)
            .SetOnInsert(x => x.ClientName, string.Empty)
            .SetOnInsert(x => x.Status, string.Empty)
            .SetOnInsert(x => x.Milestones, new List<ContractMilestoneReadModel>())
            .SetOnInsert(x => x.Attachments, new List<ContractAttachmentReadModel>());

        await col.UpdateOneAsync(filter, ensureDocument, new UpdateOptions { IsUpsert = true }, ct);

        var pull = Builders<ContractReadModel>.Update.PullFilter(
            x => x.Milestones,
            m => m.MilestoneId == milestoneId
        );

        await col.UpdateOneAsync(filter, pull, cancellationToken: ct);

        var milestone = new ContractMilestoneReadModel
        {
            MilestoneId = milestoneId,
            Description = description,
            Percentage = percentage,
            Amount = amount,
            ScheduledDate = ToUtcDate(scheduledDate),
            IsBilled = isBilled,
            InvoiceId = invoiceId,
        };

        var push = Builders<ContractReadModel>.Update.Push(x => x.Milestones, milestone);

        await col.UpdateOneAsync(filter, push, cancellationToken: ct);
    }

    private async Task UpsertAttachmentAsync(
        Guid contractId,
        Guid attachmentId,
        string fileName,
        string contentType,
        long sizeBytes,
        string storagePath,
        DateTime uploadedAtUtc,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);

        var id = $"contract:{contractId}";
        var filter = Builders<ContractReadModel>.Filter.Eq(x => x.Id, id);

        var ensureDocument = Builders<ContractReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ContractId, contractId)
            .SetOnInsert(x => x.Code, string.Empty)
            .SetOnInsert(x => x.ClientId, Guid.Empty)
            .SetOnInsert(x => x.ClientName, string.Empty)
            .SetOnInsert(x => x.Status, string.Empty)
            .SetOnInsert(x => x.Milestones, new List<ContractMilestoneReadModel>())
            .SetOnInsert(x => x.Attachments, new List<ContractAttachmentReadModel>());

        await col.UpdateOneAsync(filter, ensureDocument, new UpdateOptions { IsUpsert = true }, ct);

        var pull = Builders<ContractReadModel>.Update.PullFilter(
            x => x.Attachments,
            a => a.AttachmentId == attachmentId
        );

        await col.UpdateOneAsync(filter, pull, cancellationToken: ct);

        var attachment = new ContractAttachmentReadModel
        {
            AttachmentId = attachmentId,
            FileName = fileName,
            ContentType = contentType,
            StorageKey = storagePath,
            Size = sizeBytes,
            UploadedAt = uploadedAtUtc,
        };

        var push = Builders<ContractReadModel>.Update.Push(x => x.Attachments, attachment);

        await col.UpdateOneAsync(filter, push, cancellationToken: ct);
    }

    private async Task<string> ResolveClientNameAsync(Guid clientId, CancellationToken ct)
    {
        var clients = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);

        var client = await clients.Find(x => x.ClientId == clientId).FirstOrDefaultAsync(ct);

        if (client is null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(client.TradeName))
            return client.TradeName.Trim();

        if (!string.IsNullOrWhiteSpace(client.ContactName))
            return client.ContactName.Trim();

        if (!string.IsNullOrWhiteSpace(client.Identification))
            return client.Identification.Trim();

        return string.Empty;
    }

    private static DateTime ToUtcDate(DateOnly value) =>
        new(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Utc);
}
