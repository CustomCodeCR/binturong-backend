using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using Application.ReadModels.Inventory;
using Application.ReadModels.MasterData;
using Application.ReadModels.Sales;
using Domain.Invoices;
using MongoDB.Driver;

namespace Infrastructure.Projections.Sales;

internal sealed class InvoiceProjection
    : IProjector<InvoiceCreatedDomainEvent>,
        IProjector<InvoiceUpdatedDomainEvent>,
        IProjector<InvoiceDeletedDomainEvent>,
        IProjector<InvoiceEmissionRequestedDomainEvent>,
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

    public InvoiceProjection(IMongoDatabase db)
    {
        _db = db;
    }

    public Task ProjectAsync(InvoiceCreatedDomainEvent e, CancellationToken ct) =>
        UpsertHeaderAsync(e, ct);

    public Task ProjectAsync(InvoiceUpdatedDomainEvent e, CancellationToken ct) =>
        UpdateTotalsAsync(e, ct);

    public async Task ProjectAsync(InvoiceDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);
        await col.DeleteOneAsync(x => x.Id == $"invoice:{e.InvoiceId}", ct);
    }

    public async Task ProjectAsync(InvoiceEmissionRequestedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);
        var id = $"invoice:{e.InvoiceId}";
        var filter = Builders<InvoiceReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<InvoiceReadModel>
            .Update.Set(x => x.TaxStatus, "Processing")
            .Set(x => x.InternalStatus, "Pending");

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
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

    private async Task UpsertHeaderAsync(InvoiceCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);

        var clientName = await ResolveClientNameAsync(e.ClientId, ct);
        var branchName = e.BranchId is null
            ? null
            : await ResolveBranchNameAsync(e.BranchId.Value, ct);

        var lines = new List<InvoiceLineReadModel>();

        foreach (var line in e.Lines)
        {
            var resolvedDescription = await ResolveProductDescriptionAsync(line.ProductId, ct);

            lines.Add(
                new InvoiceLineReadModel
                {
                    InvoiceDetailId = line.InvoiceDetailId,
                    ProductId = line.ProductId,
                    Description = !string.IsNullOrWhiteSpace(resolvedDescription)
                        ? resolvedDescription
                        : (
                            string.IsNullOrWhiteSpace(line.Description)
                                ? string.Empty
                                : line.Description.Trim()
                        ),
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountPerc = line.DiscountPerc,
                    TaxPerc = line.TaxPerc,
                    LineTotal = line.LineTotal,
                }
            );
        }

        var id = $"invoice:{e.InvoiceId}";
        var filter = Builders<InvoiceReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<InvoiceReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .Set(x => x.InvoiceId, e.InvoiceId)
            .Set(x => x.ClientId, e.ClientId)
            .Set(x => x.ClientName, clientName)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.BranchName, branchName)
            .Set(x => x.SalesOrderId, e.SalesOrderId)
            .Set(x => x.ContractId, e.ContractId)
            .Set(x => x.IssueDate, EnsureUtc(e.IssueDate))
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
            .Set(x => x.PaidAmount, 0m)
            .Set(x => x.PendingAmount, e.Total)
            .Set(x => x.Lines, lines);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task UpdateTotalsAsync(InvoiceUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);

        var clientName = await ResolveClientNameAsync(e.ClientId, ct);
        var branchName = e.BranchId is null
            ? null
            : await ResolveBranchNameAsync(e.BranchId.Value, ct);

        var id = $"invoice:{e.InvoiceId}";
        var filter = Builders<InvoiceReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<InvoiceReadModel>
            .Update.Set(x => x.ClientId, e.ClientId)
            .Set(x => x.ClientName, clientName)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.BranchName, branchName)
            .Set(x => x.SalesOrderId, e.SalesOrderId)
            .Set(x => x.ContractId, e.ContractId)
            .Set(x => x.IssueDate, EnsureUtc(e.IssueDate))
            .Set(x => x.DocumentType, e.DocumentType)
            .Set(x => x.Currency, e.Currency)
            .Set(x => x.ExchangeRate, e.ExchangeRate)
            .Set(x => x.Subtotal, e.Subtotal)
            .Set(x => x.Taxes, e.Taxes)
            .Set(x => x.Discounts, e.Discounts)
            .Set(x => x.Total, e.Total);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
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

    private async Task<string> ResolveBranchNameAsync(Guid branchId, CancellationToken ct)
    {
        var branches = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);

        var branch = await branches.Find(x => x.BranchId == branchId).FirstOrDefaultAsync(ct);

        if (branch is null)
            return string.Empty;

        return string.IsNullOrWhiteSpace(branch.Name) ? string.Empty : branch.Name.Trim();
    }

    private async Task<string> ResolveProductDescriptionAsync(Guid productId, CancellationToken ct)
    {
        var products = _db.GetCollection<ProductReadModel>(MongoCollections.Products);

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

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
    }
}
