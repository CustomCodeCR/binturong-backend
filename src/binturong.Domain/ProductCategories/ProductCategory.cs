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

    public void RaiseCreated() =>
        Raise(
            new ProductCategoryCreatedDomainEvent(
                Id,
                Name,
                string.IsNullOrWhiteSpace(Description) ? null : Description,
                IsActive
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new ProductCategoryUpdatedDomainEvent(
                Id,
                Name,
                string.IsNullOrWhiteSpace(Description) ? null : Description,
                IsActive
            )
        );

    public void RaiseDeleted() => Raise(new ProductCategoryDeletedDomainEvent(Id));
}
