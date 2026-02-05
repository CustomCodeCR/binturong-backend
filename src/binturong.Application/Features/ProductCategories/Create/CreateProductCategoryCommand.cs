using Application.Abstractions.Messaging;

namespace Application.Features.ProductCategories.Create;

public sealed record CreateProductCategoryCommand(
    string Name,
    string? Description,
    bool IsActive = true
) : ICommand<Guid>;
