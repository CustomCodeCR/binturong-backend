using Application.Abstractions.Messaging;

namespace Application.Features.ProductCategories.Update;

public sealed record UpdateProductCategoryCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    bool IsActive
) : ICommand;
