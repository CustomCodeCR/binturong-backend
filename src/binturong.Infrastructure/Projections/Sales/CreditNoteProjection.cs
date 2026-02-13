using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using Domain.CreditNotes;
using MongoDB.Driver;

namespace Infrastructure.Projections.Sales;

internal sealed class CreditNoteProjection
    : IProjector<CreditNoteCreatedDomainEvent>,
        IProjector<CreditNoteUpdatedDomainEvent>,
        IProjector<CreditNoteEmittedDomainEvent>,
        IProjector<CreditNoteContingencyActivatedDomainEvent>,
        IProjector<CreditNoteDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public CreditNoteProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(CreditNoteCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<CreditNoteReadModel>(MongoCollections.CreditNotes);

        var id = $"credit_note:{e.CreditNoteId}";
        var filter = Builders<CreditNoteReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<CreditNoteReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.CreditNoteId, e.CreditNoteId)
            .Set(x => x.InvoiceId, e.InvoiceId)
            .Set(x => x.InvoiceConsecutive, null)
            .Set(x => x.TaxKey, null)
            .Set(x => x.Consecutive, null)
            .Set(x => x.IssueDate, e.IssueDateUtc)
            .Set(x => x.Reason, e.Reason)
            .Set(x => x.TotalAmount, e.TotalAmount)
            .Set(x => x.TaxStatus, "Draft");

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(CreditNoteUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<CreditNoteReadModel>(MongoCollections.CreditNotes);

        var id = $"credit_note:{e.CreditNoteId}";
        var filter = Builders<CreditNoteReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<CreditNoteReadModel>
            .Update.Set(x => x.InvoiceId, e.InvoiceId)
            .Set(x => x.TaxKey, string.IsNullOrWhiteSpace(e.TaxKey) ? null : e.TaxKey)
            .Set(
                x => x.Consecutive,
                string.IsNullOrWhiteSpace(e.Consecutive) ? null : e.Consecutive
            )
            .Set(x => x.IssueDate, e.UpdatedAtUtc) // si querés mantener IssueDate original, quitalo
            .Set(x => x.Reason, e.Reason)
            .Set(x => x.TotalAmount, e.TotalAmount)
            .Set(x => x.TaxStatus, e.TaxStatus);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(
        CreditNoteContingencyActivatedDomainEvent e,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<CreditNoteReadModel>(MongoCollections.CreditNotes);

        var id = $"credit_note:{e.CreditNoteId}";
        var filter = Builders<CreditNoteReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<CreditNoteReadModel>.Update.Set(x => x.TaxStatus, "Contingency");

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(CreditNoteEmittedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<CreditNoteReadModel>(MongoCollections.CreditNotes);

        var id = $"credit_note:{e.CreditNoteId}";
        var filter = Builders<CreditNoteReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<CreditNoteReadModel>
            .Update.Set(x => x.TaxKey, e.TaxKey)
            .Set(x => x.Consecutive, e.Consecutive)
            .Set(x => x.PdfS3Key, e.PdfS3Key)
            .Set(x => x.XmlS3Key, e.XmlS3Key)
            .Set(x => x.TaxStatus, "Emitted");

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(CreditNoteDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<CreditNoteReadModel>(MongoCollections.CreditNotes);
        await col.DeleteOneAsync(x => x.Id == $"credit_note:{e.CreditNoteId}", ct);
    }
}
