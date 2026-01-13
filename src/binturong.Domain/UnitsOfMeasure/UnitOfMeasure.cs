using SharedKernel;

namespace Domain.UnitsOfMeasure;

public sealed class UnitOfMeasure : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Navigation
    public ICollection<Domain.Products.Product> Products { get; set; } =
        new List<Domain.Products.Product>();
}
