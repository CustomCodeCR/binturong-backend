namespace Api.Endpoints.Products;

public sealed record CreateProductRequest(
    string SKU,
    string? Barcode,
    string Name,
    string? Description,
    Guid? CategoryId,
    Guid? UomId,
    Guid? TaxId,
    decimal BasePrice,
    decimal AverageCost,
    bool IsService,
    bool IsActive = true
);

public sealed record UpdateProductRequest(
    string SKU,
    string? Barcode,
    string Name,
    string? Description,
    Guid? CategoryId,
    Guid? UomId,
    Guid? TaxId,
    decimal BasePrice,
    decimal AverageCost,
    bool IsService,
    bool IsActive
);
