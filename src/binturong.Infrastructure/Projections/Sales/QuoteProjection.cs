using Application.Abstractions.Projections;
using Application.ReadModels.Sales;
using Domain.Quotes;
using MongoDB.Driver;

namespace Infrastructure.Projections.Sales;

internal sealed class QuoteProjection
    : IProjector<QuoteCreatedDomainEvent>,
        IProjector<QuoteDetailAddedDomainEvent>,
        IProjector<QuoteSentDomainEvent>,
        IProjector<QuoteAcceptedDomainEvent>,
        IProjector<QuoteRejectedDomainEvent>,
        IProjector<QuoteExpiredDomainEvent>
{
    private readonly IMongoDatabase _db;

    public QuoteProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(QuoteCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<QuoteReadModel>("quotes");

        var doc = new QuoteReadModel
        {
            Id = $"quote:{e.QuoteId}",
            QuoteId = e.QuoteId,
            Code = e.Code,
            ClientId = e.ClientId,
            BranchId = e.BranchId,
            IssueDate = e.IssueDate,
            ValidUntil = e.ValidUntil,
            Status = e.Status,
            Currency = e.Currency,
            ExchangeRate = e.ExchangeRate,
            Subtotal = e.Subtotal,
            Taxes = e.Taxes,
            Discounts = e.Discounts,
            Total = e.Total,
            Lines = new List<QuoteLineReadModel>(),
        };

        await col.InsertOneAsync(doc, new InsertOneOptions(), ct);
    }

    public async Task ProjectAsync(QuoteDetailAddedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<QuoteReadModel>("quotes");

        var update = Builders<QuoteReadModel>.Update.Push(
            x => x.Lines,
            new QuoteLineReadModel
            {
                QuoteDetailId = e.QuoteDetailId,
                ProductId = e.ProductId,
                Quantity = e.Quantity,
                UnitPrice = e.UnitPrice,
                DiscountPerc = e.DiscountPerc,
                TaxPerc = e.TaxPerc,
                LineTotal = e.LineTotal,
            }
        );

        await col.UpdateOneAsync(
            x => x.QuoteId == e.QuoteId,
            update,
            options: null,
            cancellationToken: ct
        );
    }

    public Task ProjectAsync(QuoteSentDomainEvent e, CancellationToken ct) =>
        SetStatus(e.QuoteId, "Sent", ct);

    public Task ProjectAsync(QuoteAcceptedDomainEvent e, CancellationToken ct) =>
        SetStatus(e.QuoteId, "Accepted", ct);

    public Task ProjectAsync(QuoteRejectedDomainEvent e, CancellationToken ct) =>
        SetStatus(e.QuoteId, "Rejected", ct);

    public Task ProjectAsync(QuoteExpiredDomainEvent e, CancellationToken ct) =>
        SetStatus(e.QuoteId, "Expired", ct);

    private async Task SetStatus(Guid quoteId, string status, CancellationToken ct)
    {
        var col = _db.GetCollection<QuoteReadModel>("quotes");

        await col.UpdateOneAsync(
            x => x.QuoteId == quoteId,
            Builders<QuoteReadModel>.Update.Set(x => x.Status, status),
            options: null,
            cancellationToken: ct
        );
    }
}
