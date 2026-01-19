namespace Infrastructure.Database.Mongo;

internal static class MongoCollections
{
    // ===== MASTER DATA =====
    public const string Branches = "branches";
    public const string Warehouses = "warehouses";
    public const string Taxes = "taxes";
    public const string UnitsOfMeasure = "units_of_measure";
    public const string ProductCategories = "product_categories";

    // ===== SECURITY =====
    public const string Users = "users";
    public const string Roles = "roles";

    // ===== CRM =====
    public const string Clients = "clients";
    public const string Suppliers = "suppliers";

    // ===== INVENTORY =====
    public const string Products = "products";
    public const string ProductStocks = "product_stocks";
    public const string InventoryMovements = "inventory_movements";

    // ===== SALES =====
    public const string Quotes = "quotes";
    public const string SalesOrders = "sales_orders";
    public const string Contracts = "contracts";
    public const string Invoices = "invoices";
    public const string Payments = "payments";
    public const string CreditNotes = "credit_notes";
    public const string DebitNotes = "debit_notes";

    // ===== PURCHASES =====
    public const string PurchaseRequests = "purchase_requests";
    public const string PurchaseOrders = "purchase_orders";
    public const string PurchaseReceipts = "purchase_receipts";

    // ===== PAYABLES =====
    public const string AccountsPayable = "accounts_payable";

    // ===== PAYROLL =====
    public const string Employees = "employees";
    public const string Payrolls = "payrolls";

    // ===== SERVICES =====
    public const string Services = "services";
    public const string ServiceOrders = "service_orders";

    // ===== E-COMMERCE =====
    public const string WebClients = "web_clients";
    public const string ShoppingCarts = "shopping_carts";

    // ===== MARKETING =====
    public const string MarketingCampaigns = "marketing_campaigns";
    public const string MarketingAssetTracking = "marketing_asset_tracking";

    // ===== PAYMENT GATEWAY =====
    public const string PaymentGateways = "payment_gateways";
    public const string GatewayTransactions = "gateway_transactions";

    // ===== ACCOUNTING =====
    public const string CostCenters = "cost_centers";
    public const string Accounts = "accounts";
    public const string AccountingPeriods = "accounting_periods";
    public const string JournalEntries = "journal_entries";

    // ===== AUDIT =====
    public const string AuditLogs = "audit_logs";
}
