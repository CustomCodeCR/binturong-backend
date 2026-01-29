using SharedKernel;

namespace Domain.Taxes;

public sealed class Tax : Entity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ===== Domain Events =====

    public void RaiseCreated() =>
        Raise(
            new TaxCreatedDomainEvent(Id, Name, Code, Percentage, IsActive, CreatedAt, UpdatedAt)
        );

    public void RaiseUpdated() =>
        Raise(new TaxUpdatedDomainEvent(Id, Name, Code, Percentage, IsActive, UpdatedAt));

    public void RaiseDeleted() => Raise(new TaxDeletedDomainEvent(Id));

    // ===== Navigation =====

    public ICollection<Domain.Products.Product> Products { get; set; } =
        new List<Domain.Products.Product>();
}
