using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Postgres;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applies ALL IEntityTypeConfiguration<> found in binturong.Infrastructure assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    // You can omit explicit DbSet properties and use Set<TEntity>(),
    // but because your interface requires them, declare them.

    public DbSet<Domain.Users.User> Users => Set<Domain.Users.User>();
    public DbSet<Domain.Roles.Role> Roles => Set<Domain.Roles.Role>();
    public DbSet<Domain.Scopes.Scope> Scopes => Set<Domain.Scopes.Scope>();
    public DbSet<Domain.UserRoles.UserRole> UserRoles => Set<Domain.UserRoles.UserRole>();
    public DbSet<Domain.RoleScopes.RoleScope> RoleScopes => Set<Domain.RoleScopes.RoleScope>();

    public DbSet<Domain.Branches.Branch> Branches => Set<Domain.Branches.Branch>();
    public DbSet<Domain.Warehouses.Warehouse> Warehouses => Set<Domain.Warehouses.Warehouse>();
    public DbSet<Domain.Taxes.Tax> Taxes => Set<Domain.Taxes.Tax>();
    public DbSet<Domain.UnitsOfMeasure.UnitOfMeasure> UnitsOfMeasure =>
        Set<Domain.UnitsOfMeasure.UnitOfMeasure>();
    public DbSet<Domain.ProductCategories.ProductCategory> ProductCategories =>
        Set<Domain.ProductCategories.ProductCategory>();

    public DbSet<Domain.Clients.Client> Clients => Set<Domain.Clients.Client>();
    public DbSet<Domain.ClientAddresses.ClientAddress> ClientAddresses =>
        Set<Domain.ClientAddresses.ClientAddress>();
    public DbSet<Domain.ClientContacts.ClientContact> ClientContacts =>
        Set<Domain.ClientContacts.ClientContact>();
    public DbSet<Domain.ClientAttachments.ClientAttachment> ClientAttachments =>
        Set<Domain.ClientAttachments.ClientAttachment>();

    public DbSet<Domain.Suppliers.Supplier> Suppliers => Set<Domain.Suppliers.Supplier>();
    public DbSet<Domain.SupplierContacts.SupplierContact> SupplierContacts =>
        Set<Domain.SupplierContacts.SupplierContact>();
    public DbSet<Domain.SupplierAttachments.SupplierAttachment> SupplierAttachments =>
        Set<Domain.SupplierAttachments.SupplierAttachment>();

    public DbSet<Domain.Employees.Employee> Employees => Set<Domain.Employees.Employee>();
    public DbSet<Domain.EmployeeHistory.EmployeeHistoryEntry> EmployeeHistory =>
        Set<Domain.EmployeeHistory.EmployeeHistoryEntry>();

    public DbSet<Domain.Products.Product> Products => Set<Domain.Products.Product>();
    public DbSet<Domain.WarehouseStocks.WarehouseStock> WarehouseStocks =>
        Set<Domain.WarehouseStocks.WarehouseStock>();
    public DbSet<Domain.InventoryMovementTypes.InventoryMovementType> InventoryMovementTypes =>
        Set<Domain.InventoryMovementTypes.InventoryMovementType>();
    public DbSet<Domain.InventoryMovements.InventoryMovement> InventoryMovements =>
        Set<Domain.InventoryMovements.InventoryMovement>();

    public DbSet<Domain.Quotes.Quote> Quotes => Set<Domain.Quotes.Quote>();
    public DbSet<Domain.QuoteDetails.QuoteDetail> QuoteDetails =>
        Set<Domain.QuoteDetails.QuoteDetail>();

    public DbSet<Domain.SalesOrders.SalesOrder> SalesOrders => Set<Domain.SalesOrders.SalesOrder>();
    public DbSet<Domain.SalesOrderDetails.SalesOrderDetail> SalesOrderDetails =>
        Set<Domain.SalesOrderDetails.SalesOrderDetail>();

    public DbSet<Domain.Contracts.Contract> Contracts => Set<Domain.Contracts.Contract>();
    public DbSet<Domain.ContractBillingMilestones.ContractBillingMilestone> ContractBillingMilestones =>
        Set<Domain.ContractBillingMilestones.ContractBillingMilestone>();

    public DbSet<Domain.Invoices.Invoice> Invoices => Set<Domain.Invoices.Invoice>();
    public DbSet<Domain.InvoiceDetails.InvoiceDetail> InvoiceDetails =>
        Set<Domain.InvoiceDetails.InvoiceDetail>();
    public DbSet<Domain.CreditNotes.CreditNote> CreditNotes => Set<Domain.CreditNotes.CreditNote>();
    public DbSet<Domain.DebitNotes.DebitNote> DebitNotes => Set<Domain.DebitNotes.DebitNote>();

    public DbSet<Domain.PaymentMethods.PaymentMethod> PaymentMethods =>
        Set<Domain.PaymentMethods.PaymentMethod>();
    public DbSet<Domain.Payments.Payment> Payments => Set<Domain.Payments.Payment>();
    public DbSet<Domain.PaymentDetails.PaymentDetail> PaymentDetails =>
        Set<Domain.PaymentDetails.PaymentDetail>();

    public DbSet<Domain.PurchaseRequests.PurchaseRequest> PurchaseRequests =>
        Set<Domain.PurchaseRequests.PurchaseRequest>();
    public DbSet<Domain.PurchaseOrders.PurchaseOrder> PurchaseOrders =>
        Set<Domain.PurchaseOrders.PurchaseOrder>();
    public DbSet<Domain.PurchaseOrderDetails.PurchaseOrderDetail> PurchaseOrderDetails =>
        Set<Domain.PurchaseOrderDetails.PurchaseOrderDetail>();
    public DbSet<Domain.PurchaseReceipts.PurchaseReceipt> PurchaseReceipts =>
        Set<Domain.PurchaseReceipts.PurchaseReceipt>();
    public DbSet<Domain.PurchaseReceiptDetails.PurchaseReceiptDetail> PurchaseReceiptDetails =>
        Set<Domain.PurchaseReceiptDetails.PurchaseReceiptDetail>();

    public DbSet<Domain.AccountsPayable.AccountPayable> AccountsPayable =>
        Set<Domain.AccountsPayable.AccountPayable>();

    public DbSet<Domain.Payrolls.Payroll> Payrolls => Set<Domain.Payrolls.Payroll>();
    public DbSet<Domain.PayrollDetails.PayrollDetail> PayrollDetails =>
        Set<Domain.PayrollDetails.PayrollDetail>();

    public DbSet<Domain.Services.Service> Services => Set<Domain.Services.Service>();
    public DbSet<Domain.ServiceOrders.ServiceOrder> ServiceOrders =>
        Set<Domain.ServiceOrders.ServiceOrder>();
    public DbSet<Domain.ServiceOrderTechnicians.ServiceOrderTechnician> ServiceOrderTechnicians =>
        Set<Domain.ServiceOrderTechnicians.ServiceOrderTechnician>();
    public DbSet<Domain.ServiceOrderServices.ServiceOrderService> ServiceOrderServices =>
        Set<Domain.ServiceOrderServices.ServiceOrderService>();
    public DbSet<Domain.ServiceOrderMaterials.ServiceOrderMaterial> ServiceOrderMaterials =>
        Set<Domain.ServiceOrderMaterials.ServiceOrderMaterial>();
    public DbSet<Domain.ServiceOrderChecklists.ServiceOrderChecklist> ServiceOrderChecklists =>
        Set<Domain.ServiceOrderChecklists.ServiceOrderChecklist>();
    public DbSet<Domain.ServiceOrderPhotos.ServiceOrderPhoto> ServiceOrderPhotos =>
        Set<Domain.ServiceOrderPhotos.ServiceOrderPhoto>();

    public DbSet<Domain.PaymentGatewayConfig.PaymentGatewayConfiguration> PaymentGatewayConfigurations =>
        Set<Domain.PaymentGatewayConfig.PaymentGatewayConfiguration>();
    public DbSet<Domain.GatewayTransactions.GatewayTransaction> GatewayTransactions =>
        Set<Domain.GatewayTransactions.GatewayTransaction>();

    public DbSet<Domain.WebClients.WebClient> WebClients => Set<Domain.WebClients.WebClient>();
    public DbSet<Domain.ShoppingCarts.ShoppingCart> ShoppingCarts =>
        Set<Domain.ShoppingCarts.ShoppingCart>();
    public DbSet<Domain.CartItems.CartItem> CartItems => Set<Domain.CartItems.CartItem>();

    public DbSet<Domain.CostCenters.CostCenter> CostCenters => Set<Domain.CostCenters.CostCenter>();
    public DbSet<Domain.AccountsChart.AccountChart> AccountsChart =>
        Set<Domain.AccountsChart.AccountChart>();
    public DbSet<Domain.AccountingPeriods.AccountingPeriod> AccountingPeriods =>
        Set<Domain.AccountingPeriods.AccountingPeriod>();
    public DbSet<Domain.JournalEntries.JournalEntry> JournalEntries =>
        Set<Domain.JournalEntries.JournalEntry>();
    public DbSet<Domain.JournalEntryDetails.JournalEntryDetail> JournalEntryDetails =>
        Set<Domain.JournalEntryDetails.JournalEntryDetail>();

    public DbSet<Domain.AuditLogs.AuditLog> AuditLogs => Set<Domain.AuditLogs.AuditLog>();
    public DbSet<Domain.OutboxMessages.OutboxMessage> OutboxMessages =>
        Set<Domain.OutboxMessages.OutboxMessage>();
}
