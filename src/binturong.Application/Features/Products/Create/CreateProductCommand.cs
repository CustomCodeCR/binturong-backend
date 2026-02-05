using Application.Abstractions.Messaging;

namespace Application.Features.Products.Create;

public sealed record CreateProductCommand(
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
) : ICommand<Guid>;
