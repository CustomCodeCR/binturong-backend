using Application.Abstractions.Projections;
using Application.ReadModels.Accounting;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using Domain.Accounting;
using MongoDB.Driver;

namespace Infrastructure.Projections.Accounting;

internal sealed class AccountingProjection
    : IProjector<AccountingEntryCreatedDomainEvent>,
        IProjector<AccountingEntryUpdatedDomainEvent>,
        IProjector<AccountingEntryDeletedDomainEvent>,
        IProjector<AccountingEntryReconciledDomainEvent>,
        IProjector<AccountingEntryUnreconciledDomainEvent>,
        IProjector<AccountingReconciliationCreatedDomainEvent>,
        IProjector<AccountingReconciliationDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public AccountingProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(AccountingEntryCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<AccountingEntryReadModel>(MongoCollections.AccountingEntries);

        var id = $"accounting_entry:{e.AccountingEntryId}";
        var filter = Builders<AccountingEntryReadModel>.Filter.Eq(x => x.Id, id);

        var clientName = e.ClientId.HasValue
            ? await ResolveClientNameAsync(e.ClientId.Value, ct)
            : null;

        var supplierName = e.SupplierId.HasValue
            ? await ResolveSupplierNameAsync(e.SupplierId.Value, ct)
            : null;

        var update = Builders<AccountingEntryReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.AccountingEntryId, e.AccountingEntryId)
            .Set(x => x.EntryType, e.EntryType)
            .Set(x => x.Amount, e.Amount)
            .Set(x => x.Detail, e.Detail)
            .Set(x => x.Category, e.Category)
            .Set(x => x.EntryDateUtc, e.EntryDateUtc)
            .Set(x => x.ClientId, e.ClientId)
            .Set(x => x.ClientName, clientName)
            .Set(x => x.SupplierId, e.SupplierId)
            .Set(x => x.SupplierName, supplierName)
            .Set(x => x.InvoiceNumber, e.InvoiceNumber)
            .Set(x => x.ReceiptFileS3Key, e.ReceiptFileS3Key)
            .Set(x => x.IsReconciled, e.IsReconciled)
            .Set(x => x.ReconciliationId, null);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(AccountingEntryUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<AccountingEntryReadModel>(MongoCollections.AccountingEntries);

        var id = $"accounting_entry:{e.AccountingEntryId}";
        var filter = Builders<AccountingEntryReadModel>.Filter.Eq(x => x.Id, id);

        var clientName = e.ClientId.HasValue
            ? await ResolveClientNameAsync(e.ClientId.Value, ct)
            : null;

        var supplierName = e.SupplierId.HasValue
            ? await ResolveSupplierNameAsync(e.SupplierId.Value, ct)
            : null;

        var update = Builders<AccountingEntryReadModel>
            .Update.Set(x => x.Amount, e.Amount)
            .Set(x => x.Detail, e.Detail)
            .Set(x => x.Category, e.Category)
            .Set(x => x.EntryDateUtc, e.EntryDateUtc)
            .Set(x => x.ClientId, e.ClientId)
            .Set(x => x.ClientName, clientName)
            .Set(x => x.SupplierId, e.SupplierId)
            .Set(x => x.SupplierName, supplierName)
            .Set(x => x.InvoiceNumber, e.InvoiceNumber)
            .Set(x => x.ReceiptFileS3Key, e.ReceiptFileS3Key)
            .Set(x => x.IsReconciled, e.IsReconciled);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(AccountingEntryDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<AccountingEntryReadModel>(MongoCollections.AccountingEntries);
        await col.DeleteOneAsync(x => x.Id == $"accounting_entry:{e.AccountingEntryId}", ct);
    }

    public async Task ProjectAsync(AccountingEntryReconciledDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<AccountingEntryReadModel>(MongoCollections.AccountingEntries);

        var filter = Builders<AccountingEntryReadModel>.Filter.Eq(
            x => x.Id,
            $"accounting_entry:{e.AccountingEntryId}"
        );

        var update = Builders<AccountingEntryReadModel>
            .Update.Set(x => x.IsReconciled, true)
            .Set(x => x.ReconciliationId, e.ReconciliationId);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(AccountingEntryUnreconciledDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<AccountingEntryReadModel>(MongoCollections.AccountingEntries);

        var filter = Builders<AccountingEntryReadModel>.Filter.Eq(
            x => x.Id,
            $"accounting_entry:{e.AccountingEntryId}"
        );

        var update = Builders<AccountingEntryReadModel>
            .Update.Set(x => x.IsReconciled, false)
            .Set(x => x.ReconciliationId, null);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(
        AccountingReconciliationCreatedDomainEvent e,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<AccountingReconciliationReadModel>(
            MongoCollections.AccountingReconciliations
        );

        var id = $"accounting_reconciliation:{e.ReconciliationId}";
        var filter = Builders<AccountingReconciliationReadModel>.Filter.Eq(x => x.Id, id);

        var detail = await ResolveAccountingEntryDetailAsync(e.AccountingEntryId, ct);

        var update = Builders<AccountingReconciliationReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ReconciliationId, e.ReconciliationId)
            .Set(x => x.AccountingEntryId, e.AccountingEntryId)
            .Set(x => x.AccountingEntryDetail, detail)
            .Set(x => x.SourceType, e.SourceType)
            .Set(x => x.SourceId, e.SourceId)
            .Set(x => x.MatchedAmount, e.MatchedAmount)
            .Set(x => x.ReconciledAtUtc, e.ReconciledAtUtc);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(
        AccountingReconciliationDeletedDomainEvent e,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<AccountingReconciliationReadModel>(
            MongoCollections.AccountingReconciliations
        );

        await col.DeleteOneAsync(
            x => x.Id == $"accounting_reconciliation:{e.ReconciliationId}",
            ct
        );
    }

    private async Task<string?> ResolveClientNameAsync(Guid clientId, CancellationToken ct)
    {
        var clients = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);

        var client = await clients.Find(x => x.ClientId == clientId).FirstOrDefaultAsync(ct);

        if (client is null)
            return null;

        return !string.IsNullOrWhiteSpace(client.TradeName) ? client.TradeName : client.ContactName;
    }

    private async Task<string?> ResolveSupplierNameAsync(Guid supplierId, CancellationToken ct)
    {
        var suppliers = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);

        var supplier = await suppliers
            .Find(x => x.SupplierId == supplierId)
            .FirstOrDefaultAsync(ct);

        if (supplier is null)
            return null;

        return !string.IsNullOrWhiteSpace(supplier.TradeName)
            ? supplier.TradeName
            : supplier.LegalName;
    }

    private async Task<string?> ResolveAccountingEntryDetailAsync(
        Guid accountingEntryId,
        CancellationToken ct
    )
    {
        var entries = _db.GetCollection<AccountingEntryReadModel>(
            MongoCollections.AccountingEntries
        );

        var entry = await entries
            .Find(x => x.AccountingEntryId == accountingEntryId)
            .FirstOrDefaultAsync(ct);

        return entry?.Detail;
    }
}
