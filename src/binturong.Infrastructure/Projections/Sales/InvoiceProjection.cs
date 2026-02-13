using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using Domain.Invoices;
using MongoDB.Driver;

namespace Infrastructure.Projections.Sales;

internal sealed class InvoiceProjection
    : IProjector<InvoiceCreatedDomainEvent>,
        IProjector<InvoiceUpdatedDomainEvent>,
        IProjector<InvoiceDeletedDomainEvent>,
        IProjector<InvoiceEmittedDomainEvent>,
        IProjector<InvoiceEmissionRejectedDomainEvent>,
        IProjector<InvoiceContingencyActivatedDomainEvent>,
        IProjector<InvoicePaymentAppliedDomainEvent>,
        IProjector<InvoicePaidDomainEvent>,
        IProjector<InvoiceCreatedFromQuoteDomainEvent>,
        IProjector<InvoicePaymentVerificationSetDomainEvent>,
        IProjector<InvoicePaymentVerificationClearedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public InvoiceProjection(IMongoDatabase db) => _db = db;

    public Task ProjectAsync(InvoiceCreatedDomainEvent e, CancellationToken ct) =>
        UpsertHeaderAsync(e, ct);

    public Task ProjectAsync(InvoiceUpdatedDomainEvent e, CancellationToken ct) =>
        UpdateTotalsAsync(e, ct);

    public async Task ProjectAsync(InvoiceDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);
        await col.DeleteOneAsync(x => x.Id == $"invoice:{e.InvoiceId}", ct);
    }

    public async Task ProjectAsync(InvoiceEmittedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);
        var id = $"invoice:{e.InvoiceId}";
        var filter = Builders<InvoiceReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<InvoiceReadModel>
            .Update.Set(x => x.TaxKey, e.TaxKey)
            .Set(x => x.Consecutive, e.Consecutive)
            .Set(x => x.PdfS3Key, e.PdfS3Key)
            .Set(x => x.XmlS3Key, e.XmlS3Key)
            .Set(x => x.TaxStatus, "Emitted")
            .Set(x => x.InternalStatus, "Ok");

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(InvoiceEmissionRejectedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);
        var id = $"invoice:{e.InvoiceId}";
        var filter = Builders<InvoiceReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<InvoiceReadModel>
            .Update.Set(x => x.TaxStatus, "Rejected")
            .Set(x => x.InternalStatus, "Error");

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(InvoiceContingencyActivatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);
        var id = $"invoice:{e.InvoiceId}";
        var filter = Builders<InvoiceReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<InvoiceReadModel>
            .Update.Set(x => x.TaxStatus, "Contingency")
            .Set(x => x.InternalStatus, "PendingResend");

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(InvoicePaymentAppliedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);
        var id = $"invoice:{e.InvoiceId}";
        var filter = Builders<InvoiceReadModel>.Filter.Eq(x => x.Id, id);

        var internalStatus = e.PendingAmount <= 0 ? "Paid" : "Pending";

        var update = Builders<InvoiceReadModel>
            .Update.Set(x => x.PaidAmount, e.PaidAmount)
            .Set(x => x.PendingAmount, e.PendingAmount)
            .Set(x => x.InternalStatus, internalStatus);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public Task ProjectAsync(InvoicePaidDomainEvent e, CancellationToken ct) => Task.CompletedTask;

    public Task ProjectAsync(InvoiceCreatedFromQuoteDomainEvent e, CancellationToken ct) =>
        Task.CompletedTask;

    private async Task UpsertHeaderAsync(InvoiceCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);

        var id = $"invoice:{e.InvoiceId}";
        var filter = Builders<InvoiceReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<InvoiceReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.InvoiceId, e.InvoiceId)
            .Set(x => x.ClientId, e.ClientId)
            .Set(x => x.ClientName, string.Empty)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.BranchName, null)
            .Set(x => x.SalesOrderId, e.SalesOrderId)
            .Set(x => x.ContractId, e.ContractId)
            .Set(x => x.IssueDate, e.IssueDate)
            .Set(x => x.DocumentType, e.DocumentType)
            .Set(x => x.Currency, e.Currency)
            .Set(x => x.ExchangeRate, e.ExchangeRate)
            .Set(x => x.Subtotal, e.Subtotal)
            .Set(x => x.Taxes, e.Taxes)
            .Set(x => x.Discounts, e.Discounts)
            .Set(x => x.Total, e.Total)
            .Set(x => x.TaxStatus, "Draft")
            .Set(x => x.InternalStatus, "Draft")
            .Set(x => x.EmailSent, false)
            .Set(x => x.PaidAmount, 0)
            .Set(x => x.PendingAmount, e.Total);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task UpdateTotalsAsync(InvoiceUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);

        var id = $"invoice:{e.InvoiceId}";
        var filter = Builders<InvoiceReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<InvoiceReadModel>
            .Update.Set(x => x.ClientId, e.ClientId)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.SalesOrderId, e.SalesOrderId)
            .Set(x => x.ContractId, e.ContractId)
            .Set(x => x.IssueDate, e.IssueDate)
            .Set(x => x.DocumentType, e.DocumentType)
            .Set(x => x.Currency, e.Currency)
            .Set(x => x.ExchangeRate, e.ExchangeRate)
            .Set(x => x.Subtotal, e.Subtotal)
            .Set(x => x.Taxes, e.Taxes)
            .Set(x => x.Discounts, e.Discounts)
            .Set(x => x.Total, e.Total);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(InvoicePaymentVerificationSetDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);
        var id = $"invoice:{e.InvoiceId}";
        var filter = Builders<InvoiceReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<InvoiceReadModel>.Update.Set(
            x => x.InternalStatus,
            "PaymentVerification"
        );

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(
        InvoicePaymentVerificationClearedDomainEvent e,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);
        var id = $"invoice:{e.InvoiceId}";
        var filter = Builders<InvoiceReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<InvoiceReadModel>.Update.Set(x => x.InternalStatus, "Pending");

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}
