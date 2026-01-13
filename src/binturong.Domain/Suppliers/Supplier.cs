using SharedKernel;

namespace Domain.Suppliers;

public sealed class Supplier : Entity
{
    public Guid Id { get; set; }
    public string IdentificationType { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
    public string MainCurrency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<Domain.SupplierContacts.SupplierContact> Contacts { get; set; } =
        new List<Domain.SupplierContacts.SupplierContact>();
    public ICollection<Domain.SupplierAttachments.SupplierAttachment> Attachments { get; set; } =
        new List<Domain.SupplierAttachments.SupplierAttachment>();

    public ICollection<Domain.PurchaseOrders.PurchaseOrder> PurchaseOrders { get; set; } =
        new List<Domain.PurchaseOrders.PurchaseOrder>();
    public ICollection<Domain.AccountsPayable.AccountPayable> AccountsPayables { get; set; } =
        new List<Domain.AccountsPayable.AccountPayable>();
}
