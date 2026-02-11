using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using Domain.SalesOrders;
using MongoDB.Driver;

namespace Infrastructure.Projections.Sales;

internal sealed class SalesOrderProjection
    : IProjector<SalesOrderCreatedDomainEvent>,
        IProjector<SalesOrderConvertedFromQuoteDomainEvent>,
        IProjector<SalesOrderDetailAddedDomainEvent>,
        IProjector<SalesOrderConfirmedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public SalesOrderProjection(IMongoDatabase db) => _db = db;

    public Task ProjectAsync(SalesOrderCreatedDomainEvent e, CancellationToken ct) =>
        UpsertHeaderAsync(e, ct);

    public Task ProjectAsync(SalesOrderConvertedFromQuoteDomainEvent e, CancellationToken ct) =>
        SetQuoteAsync(e.SalesOrderId, e.QuoteId, ct);

    public Task ProjectAsync(SalesOrderConfirmedDomainEvent e, CancellationToken ct) =>
        SetStatusAsync(e.SalesOrderId, "Confirmed", ct);

    public Task ProjectAsync(SalesOrderDetailAddedDomainEvent e, CancellationToken ct) =>
        AddOrReplaceLineAsync(e, ct);

    private async Task UpsertHeaderAsync(SalesOrderCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var id = $"so:{e.SalesOrderId}";
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<SalesOrderReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.SalesOrderId, e.SalesOrderId)
            .Set(x => x.Code, e.Code)
            .Set(x => x.QuoteId, e.QuoteId)
            .Set(x => x.ClientId, e.ClientId)
            .Set(x => x.ClientName, string.Empty) // fill later if you project client snapshot
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.BranchName, null) // fill later if you project branch snapshot
            .Set(x => x.OrderDate, e.OrderDateUtc)
            .Set(x => x.Status, e.Status)
            .Set(x => x.Currency, e.Currency)
            .Set(x => x.ExchangeRate, e.ExchangeRate)
            .Set(x => x.Subtotal, e.Subtotal)
            .Set(x => x.Taxes, e.Taxes)
            .Set(x => x.Discounts, e.Discounts)
            .Set(x => x.Total, e.Total);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task SetQuoteAsync(Guid salesOrderId, Guid quoteId, CancellationToken ct)
    {
        var col = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var id = $"so:{salesOrderId}";
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<SalesOrderReadModel>.Update.Set(x => x.QuoteId, quoteId);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task SetStatusAsync(Guid salesOrderId, string status, CancellationToken ct)
    {
        var col = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var id = $"so:{salesOrderId}";
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<SalesOrderReadModel>.Update.Set(x => x.Status, status);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task AddOrReplaceLineAsync(
        SalesOrderDetailAddedDomainEvent e,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var id = $"so:{e.SalesOrderId}";
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.Id, id);

        var line = new SalesOrderLineReadModel
        {
            SalesOrderDetailId = e.SalesOrderDetailId,
            ProductId = e.ProductId,
            ProductName = string.Empty, // fill later if you project product snapshot
            Quantity = e.Quantity,
            UnitPrice = e.UnitPrice,
            DiscountPerc = e.DiscountPerc,
            TaxPerc = e.TaxPerc,
            LineTotal = e.LineTotal,
        };

        var pull = Builders<SalesOrderReadModel>.Update.PullFilter(
            x => x.Lines,
            l => l.SalesOrderDetailId == line.SalesOrderDetailId
        );

        await col.UpdateOneAsync(filter, pull, new UpdateOptions { IsUpsert = true }, ct);

        var push = Builders<SalesOrderReadModel>.Update.Push(x => x.Lines, line);

        await col.UpdateOneAsync(filter, push, new UpdateOptions { IsUpsert = true }, ct);
    }
}
