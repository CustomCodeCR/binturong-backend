using Application.Abstractions.Projections;
using Application.ReadModels.CRM;
using Application.ReadModels.Inventory;
using Application.ReadModels.MasterData;
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
    private const string QuotesCollection = "quotes";
    private const string ClientsCollection = "clients";
    private const string BranchesCollection = "branches";
    private const string ProductsCollection = "products";

    private readonly IMongoDatabase _db;

    public QuoteProjection(IMongoDatabase db)
    {
        _db = db;
    }

    public async Task ProjectAsync(QuoteCreatedDomainEvent e, CancellationToken ct)
    {
        var quotes = _db.GetCollection<QuoteReadModel>(QuotesCollection);

        var clientName = await ResolveClientNameAsync(e.ClientId, ct);
        var branchName = await ResolveBranchNameAsync(e.BranchId, ct);

        var doc = new QuoteReadModel
        {
            Id = $"quote:{e.QuoteId}",
            QuoteId = e.QuoteId,
            Code = e.Code,

            ClientId = e.ClientId,
            ClientName = clientName,

            BranchId = e.BranchId,
            BranchName = branchName,

            IssueDate = e.IssueDate,
            ValidUntil = e.ValidUntil,
            Status = e.Status,

            Currency = e.Currency,
            ExchangeRate = e.ExchangeRate,

            Subtotal = ToDecimal(e.Subtotal),
            Taxes = ToDecimal(e.Taxes),
            Discounts = ToDecimal(e.Discounts),
            Total = ToDecimal(e.Total),

            Notes = e.Notes,

            AcceptedByClient = false,
            AcceptanceDate = null,

            Version = 0,
            Lines = new List<QuoteLineReadModel>(),
        };

        await quotes.InsertOneAsync(doc, new InsertOneOptions(), ct);
    }

    public async Task ProjectAsync(QuoteDetailAddedDomainEvent e, CancellationToken ct)
    {
        var quotes = _db.GetCollection<QuoteReadModel>(QuotesCollection);

        var productName = await ResolveProductNameAsync(e.ProductId, ct);

        var line = new QuoteLineReadModel
        {
            QuoteDetailId = e.QuoteDetailId,
            ProductId = e.ProductId,
            ProductName = productName,
            Quantity = ToDecimal(e.Quantity),
            UnitPrice = ToDecimal(e.UnitPrice),
            DiscountPerc = ToDecimal(e.DiscountPerc),
            TaxPerc = ToDecimal(e.TaxPerc),
            LineTotal = ToDecimal(e.LineTotal),
        };

        var quantity = ToDecimal(e.Quantity);
        var unitPrice = ToDecimal(e.UnitPrice);
        var discountPerc = ToDecimal(e.DiscountPerc);
        var taxPerc = ToDecimal(e.TaxPerc);
        var lineTotal = ToDecimal(e.LineTotal);

        var lineSubtotal = quantity * unitPrice;
        var discountAmount = lineSubtotal * (discountPerc / 100m);
        var taxableAmount = lineSubtotal - discountAmount;
        var taxAmount = taxableAmount * (taxPerc / 100m);

        var update = Builders<QuoteReadModel>
            .Update.Push(x => x.Lines, line)
            .Inc(x => x.Subtotal, lineSubtotal)
            .Inc(x => x.Discounts, discountAmount)
            .Inc(x => x.Taxes, taxAmount)
            .Inc(x => x.Total, lineTotal)
            .Inc(x => x.Version, 1);

        await quotes.UpdateOneAsync(
            x => x.QuoteId == e.QuoteId,
            update,
            options: null,
            cancellationToken: ct
        );
    }

    public async Task ProjectAsync(QuoteSentDomainEvent e, CancellationToken ct)
    {
        await SetStatusAsync(e.QuoteId, "Sent", ct);
    }

    public async Task ProjectAsync(QuoteAcceptedDomainEvent e, CancellationToken ct)
    {
        var quotes = _db.GetCollection<QuoteReadModel>(QuotesCollection);

        var update = Builders<QuoteReadModel>
            .Update.Set(x => x.Status, "Accepted")
            .Set(x => x.AcceptedByClient, true)
            .Set(x => x.AcceptanceDate, e.AcceptedAt)
            .Inc(x => x.Version, 1);

        await quotes.UpdateOneAsync(
            x => x.QuoteId == e.QuoteId,
            update,
            options: null,
            cancellationToken: ct
        );
    }

    public async Task ProjectAsync(QuoteRejectedDomainEvent e, CancellationToken ct)
    {
        var quotes = _db.GetCollection<QuoteReadModel>(QuotesCollection);

        var update = Builders<QuoteReadModel>
            .Update.Set(x => x.Status, "Rejected")
            .Set(x => x.AcceptedByClient, false)
            .Set(x => x.AcceptanceDate, null)
            .Inc(x => x.Version, 1);

        await quotes.UpdateOneAsync(
            x => x.QuoteId == e.QuoteId,
            update,
            options: null,
            cancellationToken: ct
        );
    }

    public async Task ProjectAsync(QuoteExpiredDomainEvent e, CancellationToken ct)
    {
        var quotes = _db.GetCollection<QuoteReadModel>(QuotesCollection);

        var update = Builders<QuoteReadModel>
            .Update.Set(x => x.Status, "Expired")
            .Inc(x => x.Version, 1);

        await quotes.UpdateOneAsync(
            x => x.QuoteId == e.QuoteId,
            update,
            options: null,
            cancellationToken: ct
        );
    }

    private async Task SetStatusAsync(Guid quoteId, string status, CancellationToken ct)
    {
        var quotes = _db.GetCollection<QuoteReadModel>(QuotesCollection);

        var update = Builders<QuoteReadModel>
            .Update.Set(x => x.Status, status)
            .Inc(x => x.Version, 1);

        await quotes.UpdateOneAsync(
            x => x.QuoteId == quoteId,
            update,
            options: null,
            cancellationToken: ct
        );
    }

    private async Task<string> ResolveClientNameAsync(Guid clientId, CancellationToken ct)
    {
        var clients = _db.GetCollection<ClientReadModel>(ClientsCollection);

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

    private async Task<string> ResolveBranchNameAsync(Guid? branchId, CancellationToken ct)
    {
        var branches = _db.GetCollection<BranchReadModel>(BranchesCollection);

        var branch = await branches.Find(x => x.BranchId == branchId).FirstOrDefaultAsync(ct);

        if (branch is null)
            return string.Empty;

        return string.IsNullOrWhiteSpace(branch.Name) ? string.Empty : branch.Name.Trim();
    }

    private async Task<string> ResolveProductNameAsync(Guid productId, CancellationToken ct)
    {
        var products = _db.GetCollection<ProductReadModel>(ProductsCollection);

        var product = await products.Find(x => x.ProductId == productId).FirstOrDefaultAsync(ct);

        if (product is null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(product.Name))
            return product.Name.Trim();

        if (!string.IsNullOrWhiteSpace(product.Description))
            return product.Description.Trim();

        if (!string.IsNullOrWhiteSpace(product.SKU))
            return product.SKU.Trim();

        return string.Empty;
    }

    private static decimal ToDecimal(object? value)
    {
        if (value is null)
            return 0m;

        return value switch
        {
            decimal d => d,
            double d => (decimal)d,
            float f => (decimal)f,
            int i => i,
            long l => l,
            string s when decimal.TryParse(s, out var parsed) => parsed,
            _ => Convert.ToDecimal(value),
        };
    }
}
