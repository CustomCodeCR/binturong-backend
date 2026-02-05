using Application.Abstractions.Messaging;

namespace Application.Features.Products.Update;

public sealed record UpdateProductCommand(
    Guid ProductId,
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
) : ICommand;
