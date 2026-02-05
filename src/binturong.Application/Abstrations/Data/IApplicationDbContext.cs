using Domain.AccountingPeriods;
using Domain.AccountsChart;
using Domain.AccountsPayable;
using Domain.AuditLogs;
using Domain.Branches;
using Domain.CartItems;
using Domain.ClientAddresses;
using Domain.ClientAttachments;
using Domain.ClientContacts;
using Domain.Clients;
using Domain.ContractBillingMilestones;
using Domain.Contracts;
using Domain.CostCenters;
using Domain.CreditNotes;
using Domain.DebitNotes;
using Domain.EmployeeHistory;
using Domain.Employees;
using Domain.GatewayTransactions;
using Domain.InventoryMovements;
using Domain.InventoryMovementTypes;
using Domain.InventoryTransfers;
using Domain.InvoiceDetails;
using Domain.Invoices;
using Domain.JournalEntries;
using Domain.JournalEntryDetails;
using Domain.OutboxMessages;
using Domain.PaymentDetails;
using Domain.PaymentGatewayConfig;
using Domain.PaymentMethods;
using Domain.Payments;
using Domain.PayrollDetails;
using Domain.Payrolls;
using Domain.ProductCategories;
using Domain.Products;
using Domain.PurchaseOrderDetails;
using Domain.PurchaseOrders;
using Domain.PurchaseReceiptDetails;
using Domain.PurchaseReceipts;
using Domain.PurchaseRequests;
using Domain.QuoteDetails;
using Domain.Quotes;
using Domain.Roles;
using Domain.RoleScopes;
using Domain.SalesOrderDetails;
using Domain.SalesOrders;
using Domain.Scopes;
using Domain.ServiceOrderChecklists;
using Domain.ServiceOrderMaterials;
using Domain.ServiceOrderPhotos;
using Domain.ServiceOrders;
using Domain.ServiceOrderServices;
using Domain.ServiceOrderTechnicians;
using Domain.Services;
using Domain.ShoppingCarts;
using Domain.SupplierAttachments;
using Domain.SupplierContacts;
using Domain.Suppliers;
using Domain.Taxes;
using Domain.UnitsOfMeasure;
using Domain.UserRoles;
using Domain.Users;
using Domain.Warehouses;
using Domain.WarehouseStocks;
using Domain.WebClients;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    // Users / Roles
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Scope> Scopes { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RoleScope> RoleScopes { get; }

    // Master data
    DbSet<Branch> Branches { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<Tax> Taxes { get; }
    DbSet<UnitOfMeasure> UnitsOfMeasure { get; }
    DbSet<ProductCategory> ProductCategories { get; }

    // Clients
    DbSet<Client> Clients { get; }
    DbSet<ClientAddress> ClientAddresses { get; }
    DbSet<ClientContact> ClientContacts { get; }
    DbSet<ClientAttachment> ClientAttachments { get; }

    // Suppliers
    DbSet<Supplier> Suppliers { get; }
    DbSet<SupplierContact> SupplierContacts { get; }
    DbSet<SupplierAttachment> SupplierAttachments { get; }

    // Employees
    DbSet<Employee> Employees { get; }
    DbSet<EmployeeHistoryEntry> EmployeeHistory { get; }

    // Inventory
    DbSet<Product> Products { get; }
    DbSet<WarehouseStock> WarehouseStocks { get; }
    DbSet<InventoryMovement> InventoryMovements { get; }
    DbSet<InventoryTransfer> InventoryTransfers { get; }
    DbSet<InventoryTransferLine> InventoryTransferLines { get; }

    // Quotes / Sales Orders
    DbSet<Quote> Quotes { get; }
    DbSet<QuoteDetail> QuoteDetails { get; }

    DbSet<SalesOrder> SalesOrders { get; }
    DbSet<SalesOrderDetail> SalesOrderDetails { get; }

    // Contracts
    DbSet<Contract> Contracts { get; }
    DbSet<ContractBillingMilestone> ContractBillingMilestones { get; }

    // Invoicing
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceDetail> InvoiceDetails { get; }
    DbSet<CreditNote> CreditNotes { get; }
    DbSet<DebitNote> DebitNotes { get; }

    // Payments
    DbSet<PaymentMethod> PaymentMethods { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentDetail> PaymentDetails { get; }

    // Purchases
    DbSet<PurchaseRequest> PurchaseRequests { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; }
    DbSet<PurchaseReceipt> PurchaseReceipts { get; }
    DbSet<PurchaseReceiptDetail> PurchaseReceiptDetails { get; }

    // Payables
    DbSet<AccountPayable> AccountsPayable { get; }

    // Payroll
    DbSet<Payroll> Payrolls { get; }
    DbSet<PayrollDetail> PayrollDetails { get; }

    // Services
    DbSet<Service> Services { get; }
    DbSet<ServiceOrder> ServiceOrders { get; }
    DbSet<ServiceOrderTechnician> ServiceOrderTechnicians { get; }
    DbSet<ServiceOrderService> ServiceOrderServices { get; }
    DbSet<ServiceOrderMaterial> ServiceOrderMaterials { get; }
    DbSet<ServiceOrderChecklist> ServiceOrderChecklists { get; }
    DbSet<ServiceOrderPhoto> ServiceOrderPhotos { get; }

    // Payment gateway / ecommerce
    DbSet<PaymentGatewayConfiguration> PaymentGatewayConfigurations { get; }
    DbSet<GatewayTransaction> GatewayTransactions { get; }

    DbSet<WebClient> WebClients { get; }
    DbSet<ShoppingCart> ShoppingCarts { get; }
    DbSet<CartItem> CartItems { get; }

    // Accounting
    DbSet<CostCenter> CostCenters { get; }
    DbSet<AccountChart> AccountsChart { get; }
    DbSet<AccountingPeriod> AccountingPeriods { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalEntryDetail> JournalEntryDetails { get; }

    // Audit + Outbox
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
