using Application.ReadModels.Accounting;
using Application.ReadModels.Audit;
using Application.ReadModels.CRM;
using Application.ReadModels.Ecommerce;
using Application.ReadModels.Inventory;
using Application.ReadModels.Marketing;
using Application.ReadModels.MasterData;
using Application.ReadModels.Payables;
using Application.ReadModels.Payments;
using Application.ReadModels.Payroll;
using Application.ReadModels.Purchases;
using Application.ReadModels.Sales;
using Application.ReadModels.Security;
using Application.ReadModels.Services;
using Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Mongo;

internal sealed class MongoIndexSeeder
{
    private readonly IMongoDatabase _database;

    public MongoIndexSeeder(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // ===== MASTER DATA =====
        await CreateUniqueIndexAsync<BranchReadModel>(MongoCollections.Branches, x => x.BranchId);

        await CreateUniqueIndexAsync<WarehouseReadModel>(
            MongoCollections.Warehouses,
            x => x.WarehouseId
        );

        // ===== SECURITY =====
        await CreateUniqueIndexAsync<UserReadModel>(MongoCollections.Users, x => x.UserId);

        await CreateIndexAsync<UserReadModel>(MongoCollections.Users, x => x.Email);

        // ===== CRM =====
        await CreateUniqueIndexAsync<ClientReadModel>(MongoCollections.Clients, x => x.ClientId);

        await CreateIndexAsync<ClientReadModel>(MongoCollections.Clients, x => x.Email);

        await CreateIndexAsync<ClientReadModel>(MongoCollections.Clients, x => x.TradeName);

        await CreateUniqueIndexAsync<SupplierReadModel>(
            MongoCollections.Suppliers,
            x => x.SupplierId
        );

        // ===== INVENTORY =====
        await CreateUniqueIndexAsync<ProductReadModel>(MongoCollections.Products, x => x.ProductId);

        await CreateIndexAsync<ProductReadModel>(MongoCollections.Products, x => x.SKU);

        await CreateUniqueIndexAsync<ProductStockReadModel>(
            MongoCollections.ProductStocks,
            x => x.ProductId
        );

        // ===== SALES =====
        await CreateUniqueIndexAsync<QuoteReadModel>(MongoCollections.Quotes, x => x.QuoteId);

        await CreateIndexAsync<QuoteReadModel>(MongoCollections.Quotes, x => x.ClientId);

        await CreateUniqueIndexAsync<SalesOrderReadModel>(
            MongoCollections.SalesOrders,
            x => x.SalesOrderId
        );

        await CreateUniqueIndexAsync<InvoiceReadModel>(MongoCollections.Invoices, x => x.InvoiceId);

        await CreateIndexAsync<InvoiceReadModel>(MongoCollections.Invoices, x => x.ClientId);

        await CreateIndexAsync<InvoiceReadModel>(MongoCollections.Invoices, x => x.IssueDate);

        // ===== PAYMENTS =====
        await CreateUniqueIndexAsync<PaymentReadModel>(MongoCollections.Payments, x => x.PaymentId);

        await CreateIndexAsync<GatewayTransactionReadModel>(
            MongoCollections.GatewayTransactions,
            x => x.InvoiceId
        );

        await CreateIndexAsync<GatewayTransactionReadModel>(
            MongoCollections.GatewayTransactions,
            x => x.TransactionDate
        );

        // ===== PURCHASES =====
        await CreateUniqueIndexAsync<PurchaseOrderReadModel>(
            MongoCollections.PurchaseOrders,
            x => x.PurchaseOrderId
        );

        // ===== PAYABLES =====
        await CreateUniqueIndexAsync<AccountsPayableReadModel>(
            MongoCollections.AccountsPayable,
            x => x.AccountPayableId
        );

        await CreateIndexAsync<AccountsPayableReadModel>(
            MongoCollections.AccountsPayable,
            x => x.DueDate
        );

        // ===== PAYROLL =====
        await CreateUniqueIndexAsync<EmployeeReadModel>(
            MongoCollections.Employees,
            x => x.EmployeeId
        );

        // ===== SERVICES =====
        await CreateUniqueIndexAsync<ServiceOrderReadModel>(
            MongoCollections.ServiceOrders,
            x => x.ServiceOrderId
        );

        // ===== E-COMMERCE =====
        await CreateUniqueIndexAsync<WebClientReadModel>(
            MongoCollections.WebClients,
            x => x.WebClientId
        );

        await CreateIndexAsync<ShoppingCartReadModel>(
            MongoCollections.ShoppingCarts,
            x => x.WebClientId
        );

        // ===== MARKETING =====
        await CreateUniqueIndexAsync<MarketingCampaignReadModel>(
            MongoCollections.MarketingCampaigns,
            x => x.CampaignId
        );

        await CreateIndexAsync<MarketingAssetTrackingReadModel>(
            MongoCollections.MarketingAssetTracking,
            x => x.AssetId
        );

        // ===== ACCOUNTING =====
        await CreateUniqueIndexAsync<AccountReadModel>(MongoCollections.Accounts, x => x.AccountId);

        await CreateUniqueIndexAsync<JournalEntryReadModel>(
            MongoCollections.JournalEntries,
            x => x.JournalEntryId
        );

        // ===== AUDIT =====
        await CreateIndexAsync<AuditLogReadModel>(MongoCollections.AuditLogs, x => x.EventDate);
    }

    // ===================== HELPERS =====================

    private async Task CreateUniqueIndexAsync<T>(
        string collectionName,
        System.Linq.Expressions.Expression<Func<T, object>> field
    )
    {
        var collection = _database.GetCollection<T>(collectionName);

        var index = new CreateIndexModel<T>(
            Builders<T>.IndexKeys.Ascending(field),
            new CreateIndexOptions { Unique = true }
        );

        await collection.Indexes.CreateOneAsync(index);
    }

    private async Task CreateIndexAsync<T>(
        string collectionName,
        System.Linq.Expressions.Expression<Func<T, object>> field
    )
    {
        var collection = _database.GetCollection<T>(collectionName);

        var index = new CreateIndexModel<T>(Builders<T>.IndexKeys.Ascending(field));

        await collection.Indexes.CreateOneAsync(index);
    }
}
