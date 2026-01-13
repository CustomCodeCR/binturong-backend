using SharedKernel;

namespace Domain.Taxes;

public sealed class Tax : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public bool IsActive { get; set; }

    // Navigation
    public ICollection<Domain.Products.Product> Products { get; set; } =
        new List<Domain.Products.Product>();
}
