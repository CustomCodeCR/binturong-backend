using SharedKernel;

namespace Domain.UnitsOfMeasure;

public sealed class UnitOfMeasure : Entity
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ===== Domain Events =====

    public void RaiseCreated() =>
        Raise(new UnitOfMeasureCreatedDomainEvent(Id, Code, Name, IsActive, CreatedAt, UpdatedAt));

    public void RaiseUpdated() =>
        Raise(new UnitOfMeasureUpdatedDomainEvent(Id, Code, Name, IsActive, UpdatedAt));

    public void RaiseDeleted() => Raise(new UnitOfMeasureDeletedDomainEvent(Id));

    // ===== Navigation =====

    public ICollection<Domain.Products.Product> Products { get; set; } =
        new List<Domain.Products.Product>();
}
