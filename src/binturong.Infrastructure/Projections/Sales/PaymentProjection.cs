using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using Domain.Payments;
using MongoDB.Driver;

namespace Infrastructure.Projections.Sales;

internal sealed class PaymentProjection
    : IProjector<PaymentCreatedDomainEvent>,
        IProjector<PaymentDeletedDomainEvent>,
        IProjector<PaymentAppliedToInvoiceDomainEvent>,
        IProjector<PaymentPosRejectedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public PaymentProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(PaymentCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PaymentReadModel>(MongoCollections.Payments);

        var id = $"payment:{e.PaymentId}";
        var filter = Builders<PaymentReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<PaymentReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.PaymentId, e.PaymentId)
            .Set(x => x.ClientId, e.ClientId)
            .Set(x => x.ClientName, string.Empty)
            .Set(x => x.PaymentMethodId, e.PaymentMethodId)
            .Set(x => x.PaymentMethodCode, string.Empty)
            .Set(x => x.PaymentMethodDescription, string.Empty)
            .Set(x => x.PaymentDate, e.PaymentDateUtc)
            .Set(x => x.TotalAmount, e.TotalAmount)
            .Set(x => x.Reference, e.Reference)
            .Set(x => x.Notes, e.Notes);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(PaymentDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PaymentReadModel>(MongoCollections.Payments);
        await col.DeleteOneAsync(x => x.Id == $"payment:{e.PaymentId}", ct);
    }

    public async Task ProjectAsync(PaymentAppliedToInvoiceDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PaymentReadModel>(MongoCollections.Payments);

        var id = $"payment:{e.PaymentId}";
        var filter = Builders<PaymentReadModel>.Filter.Eq(x => x.Id, id);

        // 1) Ensure document exists (important if events arrive out of order)
        var ensure = Builders<PaymentReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.PaymentId, e.PaymentId);

        await col.UpdateOneAsync(filter, ensure, new UpdateOptions { IsUpsert = true }, ct);

        // 2) Remove previous entry for this invoiceId
        var pull = Builders<PaymentReadModel>.Update.PullFilter(
            x => x.AppliedInvoices,
            x => x.InvoiceId == e.InvoiceId
        );

        await col.UpdateOneAsync(filter, pull, new UpdateOptions { IsUpsert = false }, ct);

        // 3) Push new entry
        var push = Builders<PaymentReadModel>.Update.Push(
            x => x.AppliedInvoices,
            new PaymentAppliedInvoiceReadModel
            {
                InvoiceId = e.InvoiceId,
                InvoiceConsecutive = e.InvoiceConsecutive,
                AppliedAmount = e.AppliedAmount,
            }
        );

        await col.UpdateOneAsync(filter, push, new UpdateOptions { IsUpsert = false }, ct);
    }

    public async Task ProjectAsync(PaymentPosRejectedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PaymentReadModel>(MongoCollections.Payments);

        var id = $"payment:{e.PaymentId}";
        var filter = Builders<PaymentReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<PaymentReadModel>.Update.Set(x => x.Notes, e.Message);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}
