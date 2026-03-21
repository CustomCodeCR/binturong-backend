using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using Application.ReadModels.Inventory;
using Application.ReadModels.MasterData;
using Application.ReadModels.Sales;
using Application.ReadModels.Security;
using Application.ReadModels.Services;
using Domain.Discounts;
using Domain.SalesOrders;
using MongoDB.Driver;

namespace Infrastructure.Projections.Sales;

internal sealed class SalesOrderProjection
    : IProjector<SalesOrderCreatedDomainEvent>,
        IProjector<SalesOrderConvertedFromQuoteDomainEvent>,
        IProjector<SalesOrderDetailAddedDomainEvent>,
        IProjector<SalesOrderConfirmedDomainEvent>,
        IProjector<SalesOrderLineDiscountAppliedDomainEvent>,
        IProjector<SalesOrderLineDiscountRemovedDomainEvent>,
        IProjector<SalesOrderGlobalDiscountAppliedDomainEvent>,
        IProjector<SalesOrderGlobalDiscountRemovedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public SalesOrderProjection(IMongoDatabase db) => _db = db;

    public Task ProjectAsync(SalesOrderCreatedDomainEvent e, CancellationToken ct) =>
        UpsertHeaderAsync(e, ct);

    public Task ProjectAsync(SalesOrderConvertedFromQuoteDomainEvent e, CancellationToken ct) =>
        SetQuoteAsync(e.SalesOrderId, e.QuoteId, e.UpdatedAtUtc, ct);

    public Task ProjectAsync(SalesOrderConfirmedDomainEvent e, CancellationToken ct) =>
        SetConfirmedAsync(e.SalesOrderId, e.SellerUserId, "Confirmed", e.UpdatedAtUtc, ct);

    public Task ProjectAsync(SalesOrderDetailAddedDomainEvent e, CancellationToken ct) =>
        AddOrReplaceLineAsync(e, ct);

    public Task ProjectAsync(SalesOrderLineDiscountAppliedDomainEvent e, CancellationToken ct) =>
        ApplyLineDiscountAsync(e, ct);

    public Task ProjectAsync(SalesOrderLineDiscountRemovedDomainEvent e, CancellationToken ct) =>
        RemoveLineDiscountAsync(e, ct);

    public Task ProjectAsync(SalesOrderGlobalDiscountAppliedDomainEvent e, CancellationToken ct) =>
        ApplyGlobalDiscountAsync(e, ct);

    public Task ProjectAsync(SalesOrderGlobalDiscountRemovedDomainEvent e, CancellationToken ct) =>
        RemoveGlobalDiscountAsync(e, ct);

    private async Task UpsertHeaderAsync(SalesOrderCreatedDomainEvent e, CancellationToken ct)
    {
        var salesOrders = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var clientName = await ResolveClientNameAsync(e.ClientId, ct);
        var branchName = e.BranchId.HasValue
            ? await ResolveBranchNameAsync(e.BranchId.Value, ct)
            : null;
        var sellerName = e.SellerUserId.HasValue
            ? await ResolveUserNameAsync(e.SellerUserId.Value, ct)
            : null;

        var id = $"so:{e.SalesOrderId}";
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<SalesOrderReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.Lines, new List<SalesOrderLineReadModel>())
            .Set(x => x.SalesOrderId, e.SalesOrderId)
            .Set(x => x.Code, e.Code)
            .Set(x => x.QuoteId, e.QuoteId)
            .Set(x => x.ClientId, e.ClientId)
            .Set(x => x.ClientName, clientName)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.BranchName, branchName)
            .Set(x => x.SellerUserId, e.SellerUserId)
            .Set(x => x.SellerName, sellerName)
            .Set(x => x.OrderDate, e.OrderDateUtc)
            .Set(x => x.Status, e.Status)
            .Set(x => x.Currency, e.Currency)
            .Set(x => x.ExchangeRate, e.ExchangeRate)
            .Set(x => x.Subtotal, e.Subtotal)
            .Set(x => x.Taxes, e.Taxes)
            .Set(x => x.Discounts, e.Discounts)
            .Set(x => x.Total, e.Total)
            .Set(x => x.GlobalDiscountPerc, 0m)
            .Set(x => x.GlobalDiscountAmount, 0m)
            .Set(x => x.GlobalDiscountReason, null)
            .Set(x => x.Notes, e.Notes)
            .Set(x => x.CreatedAt, e.CreatedAtUtc)
            .Set(x => x.UpdatedAt, e.UpdatedAtUtc);

        await salesOrders.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task SetQuoteAsync(
        Guid salesOrderId,
        Guid quoteId,
        DateTime updatedAtUtc,
        CancellationToken ct
    )
    {
        var salesOrders = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var id = $"so:{salesOrderId}";
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<SalesOrderReadModel>
            .Update.Set(x => x.QuoteId, quoteId)
            .Set(x => x.UpdatedAt, updatedAtUtc);

        await salesOrders.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task SetConfirmedAsync(
        Guid salesOrderId,
        Guid sellerUserId,
        string status,
        DateTime updatedAtUtc,
        CancellationToken ct
    )
    {
        var salesOrders = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var sellerName = await ResolveUserNameAsync(sellerUserId, ct);

        var id = $"so:{salesOrderId}";
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<SalesOrderReadModel>
            .Update.Set(x => x.SellerUserId, sellerUserId)
            .Set(x => x.SellerName, sellerName)
            .Set(x => x.Status, status)
            .Set(x => x.UpdatedAt, updatedAtUtc);

        await salesOrders.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task AddOrReplaceLineAsync(
        SalesOrderDetailAddedDomainEvent e,
        CancellationToken ct
    )
    {
        var salesOrders = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var itemType = NormalizeItemType(e.ItemType);
        var itemName = await ResolveItemNameAsync(itemType, e.ProductId, e.ServiceId, ct);

        var id = $"so:{e.SalesOrderId}";
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.Id, id);

        var line = new SalesOrderLineReadModel
        {
            SalesOrderDetailId = e.SalesOrderDetailId,
            ItemType = itemType,
            ProductId = e.ProductId,
            ServiceId = e.ServiceId,
            ItemName = itemName,
            Quantity = e.Quantity,
            UnitPrice = e.UnitPrice,
            DiscountPerc = e.DiscountPerc,
            DiscountAmount = (e.Quantity * e.UnitPrice) * (e.DiscountPerc / 100m),
            DiscountReason = null,
            TaxPerc = e.TaxPerc,
            LineTotal = e.LineTotal,
        };

        var pull = Builders<SalesOrderReadModel>.Update.PullFilter(
            x => x.Lines,
            l => l.SalesOrderDetailId == line.SalesOrderDetailId
        );

        await salesOrders.UpdateOneAsync(filter, pull, new UpdateOptions { IsUpsert = true }, ct);

        var push = Builders<SalesOrderReadModel>
            .Update.Push(x => x.Lines, line)
            .Set(x => x.UpdatedAt, e.UpdatedAtUtc);

        await salesOrders.UpdateOneAsync(filter, push, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task ApplyLineDiscountAsync(
        SalesOrderLineDiscountAppliedDomainEvent e,
        CancellationToken ct
    )
    {
        var salesOrders = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.Id, $"so:{e.SalesOrderId}");

        var so = await salesOrders.Find(filter).FirstOrDefaultAsync(ct);
        if (so is null)
            return;

        var updatedLines = so
            .Lines.Select(l =>
                l.SalesOrderDetailId == e.SalesOrderDetailId
                    ? new SalesOrderLineReadModel
                    {
                        SalesOrderDetailId = l.SalesOrderDetailId,
                        ItemType = l.ItemType,
                        ProductId = l.ProductId,
                        ServiceId = l.ServiceId,
                        ItemName = l.ItemName,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountPerc = e.DiscountPerc,
                        DiscountAmount = e.DiscountAmount,
                        DiscountReason = e.Reason,
                        TaxPerc = l.TaxPerc,
                        LineTotal = l.LineTotal - e.DiscountAmount,
                    }
                    : l
            )
            .ToList();

        var subtotal = updatedLines.Sum(x => x.Quantity * x.UnitPrice);
        var lineDiscounts = updatedLines.Sum(x => x.DiscountAmount);
        var taxes = updatedLines.Sum(x =>
        {
            var baseAmount = (x.Quantity * x.UnitPrice) - x.DiscountAmount;
            return baseAmount * (x.TaxPerc / 100m);
        });

        var globalDiscountAmount = (subtotal - lineDiscounts) * (so.GlobalDiscountPerc / 100m);
        var totalDiscounts = lineDiscounts + globalDiscountAmount;
        var total = (subtotal - totalDiscounts) + taxes;

        var update = Builders<SalesOrderReadModel>
            .Update.Set(x => x.Lines, updatedLines)
            .Set(x => x.Subtotal, subtotal)
            .Set(x => x.Taxes, taxes)
            .Set(x => x.Discounts, totalDiscounts)
            .Set(x => x.Total, total)
            .Set(x => x.GlobalDiscountAmount, globalDiscountAmount)
            .Set(x => x.UpdatedAt, e.AppliedAtUtc);

        await salesOrders.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task RemoveLineDiscountAsync(
        SalesOrderLineDiscountRemovedDomainEvent e,
        CancellationToken ct
    )
    {
        var salesOrders = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.Id, $"so:{e.SalesOrderId}");

        var so = await salesOrders.Find(filter).FirstOrDefaultAsync(ct);
        if (so is null)
            return;

        var updatedLines = so
            .Lines.Select(l =>
                l.SalesOrderDetailId == e.SalesOrderDetailId
                    ? new SalesOrderLineReadModel
                    {
                        SalesOrderDetailId = l.SalesOrderDetailId,
                        ItemType = l.ItemType,
                        ProductId = l.ProductId,
                        ServiceId = l.ServiceId,
                        ItemName = l.ItemName,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountPerc = 0m,
                        DiscountAmount = 0m,
                        DiscountReason = null,
                        TaxPerc = l.TaxPerc,
                        LineTotal =
                            (l.Quantity * l.UnitPrice)
                            + ((l.Quantity * l.UnitPrice) * (l.TaxPerc / 100m)),
                    }
                    : l
            )
            .ToList();

        var subtotal = updatedLines.Sum(x => x.Quantity * x.UnitPrice);
        var lineDiscounts = updatedLines.Sum(x => x.DiscountAmount);
        var taxes = updatedLines.Sum(x =>
        {
            var baseAmount = (x.Quantity * x.UnitPrice) - x.DiscountAmount;
            return baseAmount * (x.TaxPerc / 100m);
        });

        var globalDiscountAmount = (subtotal - lineDiscounts) * (so.GlobalDiscountPerc / 100m);
        var totalDiscounts = lineDiscounts + globalDiscountAmount;
        var total = (subtotal - totalDiscounts) + taxes;

        var update = Builders<SalesOrderReadModel>
            .Update.Set(x => x.Lines, updatedLines)
            .Set(x => x.Subtotal, subtotal)
            .Set(x => x.Taxes, taxes)
            .Set(x => x.Discounts, totalDiscounts)
            .Set(x => x.Total, total)
            .Set(x => x.GlobalDiscountAmount, globalDiscountAmount)
            .Set(x => x.UpdatedAt, e.RemovedAtUtc);

        await salesOrders.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task ApplyGlobalDiscountAsync(
        SalesOrderGlobalDiscountAppliedDomainEvent e,
        CancellationToken ct
    )
    {
        var salesOrders = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.Id, $"so:{e.SalesOrderId}");

        var so = await salesOrders.Find(filter).FirstOrDefaultAsync(ct);
        if (so is null)
            return;

        var subtotal = so.Lines.Sum(x => x.Quantity * x.UnitPrice);
        var lineDiscounts = so.Lines.Sum(x => x.DiscountAmount);
        var taxes = so.Lines.Sum(x =>
        {
            var baseAmount = (x.Quantity * x.UnitPrice) - x.DiscountAmount;
            return baseAmount * (x.TaxPerc / 100m);
        });

        var totalDiscounts = lineDiscounts + e.DiscountAmount;
        var total = (subtotal - totalDiscounts) + taxes;

        var update = Builders<SalesOrderReadModel>
            .Update.Set(x => x.GlobalDiscountPerc, e.DiscountPerc)
            .Set(x => x.GlobalDiscountAmount, e.DiscountAmount)
            .Set(x => x.GlobalDiscountReason, e.Reason)
            .Set(x => x.Subtotal, subtotal)
            .Set(x => x.Taxes, taxes)
            .Set(x => x.Discounts, totalDiscounts)
            .Set(x => x.Total, total)
            .Set(x => x.UpdatedAt, e.AppliedAtUtc);

        await salesOrders.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task RemoveGlobalDiscountAsync(
        SalesOrderGlobalDiscountRemovedDomainEvent e,
        CancellationToken ct
    )
    {
        var salesOrders = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);
        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.Id, $"so:{e.SalesOrderId}");

        var so = await salesOrders.Find(filter).FirstOrDefaultAsync(ct);
        if (so is null)
            return;

        var subtotal = so.Lines.Sum(x => x.Quantity * x.UnitPrice);
        var lineDiscounts = so.Lines.Sum(x => x.DiscountAmount);
        var taxes = so.Lines.Sum(x =>
        {
            var baseAmount = (x.Quantity * x.UnitPrice) - x.DiscountAmount;
            return baseAmount * (x.TaxPerc / 100m);
        });

        var total = (subtotal - lineDiscounts) + taxes;

        var update = Builders<SalesOrderReadModel>
            .Update.Set(x => x.GlobalDiscountPerc, 0m)
            .Set(x => x.GlobalDiscountAmount, 0m)
            .Set(x => x.GlobalDiscountReason, null)
            .Set(x => x.Subtotal, subtotal)
            .Set(x => x.Taxes, taxes)
            .Set(x => x.Discounts, lineDiscounts)
            .Set(x => x.Total, total)
            .Set(x => x.UpdatedAt, e.RemovedAtUtc);

        await salesOrders.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private static string NormalizeItemType(string itemType)
    {
        if (string.Equals(itemType, "Service", StringComparison.OrdinalIgnoreCase))
            return "Service";

        return "Product";
    }

    private async Task<string> ResolveItemNameAsync(
        string itemType,
        Guid? productId,
        Guid? serviceId,
        CancellationToken ct
    )
    {
        if (itemType == "Service")
        {
            if (!serviceId.HasValue)
                return string.Empty;

            return await ResolveServiceNameAsync(serviceId.Value, ct);
        }

        if (!productId.HasValue)
            return string.Empty;

        return await ResolveProductNameAsync(productId.Value, ct);
    }

    private async Task<string> ResolveClientNameAsync(Guid clientId, CancellationToken ct)
    {
        var clients = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var client = await clients.Find(x => x.ClientId == clientId).FirstOrDefaultAsync(ct);

        if (client is null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(client.TradeName))
            return client.TradeName;

        if (!string.IsNullOrWhiteSpace(client.ContactName))
            return client.ContactName;

        return client.Identification;
    }

    private async Task<string?> ResolveBranchNameAsync(Guid branchId, CancellationToken ct)
    {
        var branches = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);
        var branch = await branches.Find(x => x.BranchId == branchId).FirstOrDefaultAsync(ct);

        if (branch is null)
            return null;

        if (!string.IsNullOrWhiteSpace(branch.Code) && !string.IsNullOrWhiteSpace(branch.Name))
            return $"{branch.Code} - {branch.Name}";

        return branch.Name;
    }

    private async Task<string?> ResolveUserNameAsync(Guid userId, CancellationToken ct)
    {
        var users = _db.GetCollection<UserReadModel>(MongoCollections.Users);
        var user = await users.Find(x => x.UserId == userId).FirstOrDefaultAsync(ct);

        if (user is null)
            return null;

        if (!string.IsNullOrWhiteSpace(user.Username))
            return user.Username;

        return user.Email;
    }

    private async Task<string> ResolveProductNameAsync(Guid productId, CancellationToken ct)
    {
        var products = _db.GetCollection<ProductReadModel>(MongoCollections.Products);
        var product = await products.Find(x => x.ProductId == productId).FirstOrDefaultAsync(ct);

        if (product is null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(product.Name))
            return product.Name;

        return product.SKU;
    }

    private async Task<string> ResolveServiceNameAsync(Guid serviceId, CancellationToken ct)
    {
        var services = _db.GetCollection<ServiceReadModel>(MongoCollections.Services);
        var service = await services.Find(x => x.ServiceId == serviceId).FirstOrDefaultAsync(ct);

        if (service is null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(service.Name))
            return service.Name;

        return service.Code;
    }
}
