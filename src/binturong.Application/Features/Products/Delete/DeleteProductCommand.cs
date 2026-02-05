using Application.Abstractions.Messaging;

namespace Application.Features.Products.Delete;

public sealed record DeleteProductCommand(Guid ProductId) : ICommand;
