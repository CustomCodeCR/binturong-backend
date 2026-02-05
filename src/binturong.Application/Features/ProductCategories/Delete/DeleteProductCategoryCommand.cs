using Application.Abstractions.Messaging;

namespace Application.Features.ProductCategories.Delete;

public sealed record DeleteProductCategoryCommand(Guid CategoryId) : ICommand;
