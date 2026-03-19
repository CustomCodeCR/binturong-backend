using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using Application.ReadModels.MasterData;
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
        var payments = _db.GetCollection<PaymentReadModel>(MongoCollections.Payments);
        var clients = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var paymentMethods = _db.GetCollection<PaymentMethodReadModel>(
            MongoCollections.PaymentMethods
        );

        var client = await clients.Find(x => x.ClientId == e.ClientId).FirstOrDefaultAsync(ct);

        var paymentMethod = await paymentMethods
            .Find(x => x.PaymentMethodId == e.PaymentMethodId)
            .FirstOrDefaultAsync(ct);

        var id = $"payment:{e.PaymentId}";
        var filter = Builders<PaymentReadModel>.Filter.Eq(x => x.Id, id);

        var clientName = !string.IsNullOrWhiteSpace(client?.TradeName)
            ? client.TradeName
            : client?.ContactName ?? string.Empty;

        var update = Builders<PaymentReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.PaymentId, e.PaymentId)
            .Set(x => x.ClientId, e.ClientId)
            .Set(x => x.ClientName, clientName)
            .Set(x => x.PaymentMethodId, e.PaymentMethodId)
            .Set(x => x.PaymentMethodCode, paymentMethod?.Code ?? string.Empty)
            .Set(x => x.PaymentMethodDescription, paymentMethod?.Description ?? string.Empty)
            .Set(x => x.PaymentDate, e.PaymentDateUtc)
            .Set(x => x.TotalAmount, e.TotalAmount)
            .Set(x => x.Reference, e.Reference)
            .Set(x => x.Notes, e.Notes);

        await payments.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(PaymentDeletedDomainEvent e, CancellationToken ct)
    {
        var payments = _db.GetCollection<PaymentReadModel>(MongoCollections.Payments);
        await payments.DeleteOneAsync(x => x.Id == $"payment:{e.PaymentId}", ct);
    }

    public async Task ProjectAsync(PaymentAppliedToInvoiceDomainEvent e, CancellationToken ct)
    {
        var payments = _db.GetCollection<PaymentReadModel>(MongoCollections.Payments);

        var id = $"payment:{e.PaymentId}";
        var filter = Builders<PaymentReadModel>.Filter.Eq(x => x.Id, id);

        var ensure = Builders<PaymentReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.PaymentId, e.PaymentId);

        await payments.UpdateOneAsync(filter, ensure, new UpdateOptions { IsUpsert = true }, ct);

        var pull = Builders<PaymentReadModel>.Update.PullFilter(
            x => x.AppliedInvoices,
            x => x.InvoiceId == e.InvoiceId
        );

        await payments.UpdateOneAsync(filter, pull, new UpdateOptions { IsUpsert = false }, ct);

        var push = Builders<PaymentReadModel>.Update.Push(
            x => x.AppliedInvoices,
            new PaymentAppliedInvoiceReadModel
            {
                InvoiceId = e.InvoiceId,
                InvoiceConsecutive = e.InvoiceConsecutive,
                AppliedAmount = e.AppliedAmount,
            }
        );

        await payments.UpdateOneAsync(filter, push, new UpdateOptions { IsUpsert = false }, ct);
    }

    public async Task ProjectAsync(PaymentPosRejectedDomainEvent e, CancellationToken ct)
    {
        var payments = _db.GetCollection<PaymentReadModel>(MongoCollections.Payments);

        var id = $"payment:{e.PaymentId}";
        var filter = Builders<PaymentReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<PaymentReadModel>.Update.Set(x => x.Notes, e.Message);

        await payments.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}
