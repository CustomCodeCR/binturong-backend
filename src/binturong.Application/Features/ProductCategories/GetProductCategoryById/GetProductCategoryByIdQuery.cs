using Application.Abstractions.Messaging;
using Application.ReadModels.MasterData;

namespace Application.Features.ProductCategories.GetProductCategoryById;

public sealed record GetProductCategoryByIdQuery(Guid CategoryId)
    : IQuery<ProductCategoryReadModel>;
