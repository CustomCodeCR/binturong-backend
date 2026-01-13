using SharedKernel;

namespace Domain.SupplierContacts;

public sealed class SupplierContact : Entity
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    public Domain.Suppliers.Supplier? Supplier { get; set; }
}
