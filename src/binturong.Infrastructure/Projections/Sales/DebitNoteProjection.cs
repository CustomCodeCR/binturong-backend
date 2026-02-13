using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using Domain.DebitNotes;
using MongoDB.Driver;

namespace Infrastructure.Projections.Sales;

internal sealed class DebitNoteProjection
    : IProjector<DebitNoteCreatedDomainEvent>,
        IProjector<DebitNoteUpdatedDomainEvent>,
        IProjector<DebitNoteEmittedDomainEvent>,
        IProjector<DebitNoteContingencyActivatedDomainEvent>,
        IProjector<DebitNoteDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public DebitNoteProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(DebitNoteCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<DebitNoteReadModel>(MongoCollections.DebitNotes);

        var id = $"debit_note:{e.DebitNoteId}";
        var filter = Builders<DebitNoteReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<DebitNoteReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.DebitNoteId, e.DebitNoteId)
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

    public async Task ProjectAsync(DebitNoteUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<DebitNoteReadModel>(MongoCollections.DebitNotes);

        var id = $"debit_note:{e.DebitNoteId}";
        var filter = Builders<DebitNoteReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<DebitNoteReadModel>
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

    public async Task ProjectAsync(DebitNoteContingencyActivatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<DebitNoteReadModel>(MongoCollections.DebitNotes);

        var id = $"debit_note:{e.DebitNoteId}";
        var filter = Builders<DebitNoteReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<DebitNoteReadModel>.Update.Set(x => x.TaxStatus, "Contingency");

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(DebitNoteEmittedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<DebitNoteReadModel>(MongoCollections.DebitNotes);

        var id = $"debit_note:{e.DebitNoteId}";
        var filter = Builders<DebitNoteReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<DebitNoteReadModel>
            .Update.Set(x => x.TaxKey, e.TaxKey)
            .Set(x => x.Consecutive, e.Consecutive)
            .Set(x => x.PdfS3Key, e.PdfS3Key)
            .Set(x => x.XmlS3Key, e.XmlS3Key)
            .Set(x => x.TaxStatus, "Emitted");

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(DebitNoteDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<DebitNoteReadModel>(MongoCollections.DebitNotes);
        await col.DeleteOneAsync(x => x.Id == $"debit_note:{e.DebitNoteId}", ct);
    }
}
