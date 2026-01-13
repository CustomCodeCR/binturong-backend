using SharedKernel;

namespace Domain.Branches;

public sealed class Branch : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<Domain.Warehouses.Warehouse> Warehouses { get; set; } =
        new List<Domain.Warehouses.Warehouse>();
    public ICollection<Domain.Employees.Employee> Employees { get; set; } =
        new List<Domain.Employees.Employee>();

    public ICollection<Domain.Quotes.Quote> Quotes { get; set; } = new List<Domain.Quotes.Quote>();
    public ICollection<Domain.SalesOrders.SalesOrder> SalesOrders { get; set; } =
        new List<Domain.SalesOrders.SalesOrder>();
    public ICollection<Domain.Invoices.Invoice> Invoices { get; set; } =
        new List<Domain.Invoices.Invoice>();

    public ICollection<Domain.PurchaseRequests.PurchaseRequest> PurchaseRequests { get; set; } =
        new List<Domain.PurchaseRequests.PurchaseRequest>();
    public ICollection<Domain.PurchaseOrders.PurchaseOrder> PurchaseOrders { get; set; } =
        new List<Domain.PurchaseOrders.PurchaseOrder>();

    public ICollection<Domain.ServiceOrders.ServiceOrder> ServiceOrders { get; set; } =
        new List<Domain.ServiceOrders.ServiceOrder>();
}
