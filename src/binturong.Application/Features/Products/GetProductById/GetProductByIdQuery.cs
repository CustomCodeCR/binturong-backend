using Application.Abstractions.Messaging;
using Application.ReadModels.Inventory;

namespace Application.Features.Products.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductReadModel>;
