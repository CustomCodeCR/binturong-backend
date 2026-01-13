using SharedKernel;

namespace Domain.ProductCategories;

public sealed class ProductCategory : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Navigation
    public ICollection<Domain.Products.Product> Products { get; set; } =
        new List<Domain.Products.Product>();
}
