namespace Api.Endpoints.ProductCategories;

public sealed record CreateProductCategoryRequest(
    string Name,
    string? Description,
    bool IsActive = true
);

public sealed record UpdateProductCategoryRequest(string Name, string? Description, bool IsActive);
