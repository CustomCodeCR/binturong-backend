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

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Domain.Suppliers.Supplier? Supplier { get; set; }

    // =========================
    // Domain events
    // =========================

    public void RaiseCreated() =>
        Raise(
            new SupplierContactCreatedDomainEvent(
                SupplierId,
                Id,
                Name,
                JobTitle,
                Email,
                Phone,
                IsPrimary,
                CreatedAt,
                UpdatedAt
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new SupplierContactUpdatedDomainEvent(
                SupplierId,
                Id,
                Name,
                JobTitle,
                Email,
                Phone,
                IsPrimary,
                UpdatedAt
            )
        );

    public void RaiseDeleted() => Raise(new SupplierContactDeletedDomainEvent(SupplierId, Id));

    public void RaisePrimarySet() =>
        Raise(new SupplierPrimaryContactSetDomainEvent(SupplierId, Id, UpdatedAt));
}
