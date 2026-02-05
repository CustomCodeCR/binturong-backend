using Application.Abstractions.Messaging;
using Application.Features.Products.Create;
using Application.Features.Products.Delete;
using Application.Features.Products.GetProductById;
using Application.Features.Products.GetProducts;
using Application.Features.Products.Update;
using Application.ReadModels.Inventory;

namespace Api.Endpoints.Products;

public sealed class ProductsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet(
            "/",
            async (
                int? page,
                int? pageSize,
                string? search,
                IQueryHandler<GetProductsQuery, IReadOnlyList<ProductReadModel>> handler,
                CancellationToken ct
            ) =>
            {
                var query = new GetProductsQuery(page ?? 1, pageSize ?? 50, search);
                var result = await handler.Handle(query, ct);
                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Ok(result.Value);
            }
        );

        group.MapGet(
            "/{id:guid}",
            async (
                Guid id,
                IQueryHandler<GetProductByIdQuery, ProductReadModel> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new GetProductByIdQuery(id), ct);
                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            }
        );

        group.MapPost(
            "/",
            async (
                CreateProductRequest req,
                ICommandHandler<CreateProductCommand, Guid> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new CreateProductCommand(
                    req.SKU,
                    req.Barcode,
                    req.Name,
                    req.Description,
                    req.CategoryId,
                    req.UomId,
                    req.TaxId,
                    req.BasePrice,
                    req.AverageCost,
                    req.IsService,
                    req.IsActive
                );

                var result = await handler.Handle(cmd, ct);
                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created(
                    $"/api/products/{result.Value}",
                    new { productId = result.Value }
                );
            }
        );

        group.MapPut(
            "/{id:guid}",
            async (
                Guid id,
                UpdateProductRequest req,
                ICommandHandler<UpdateProductCommand> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new UpdateProductCommand(
                    id,
                    req.SKU,
                    req.Barcode,
                    req.Name,
                    req.Description,
                    req.CategoryId,
                    req.UomId,
                    req.TaxId,
                    req.BasePrice,
                    req.AverageCost,
                    req.IsService,
                    req.IsActive
                );

                var result = await handler.Handle(cmd, ct);
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        group.MapDelete(
            "/{id:guid}",
            async (Guid id, ICommandHandler<DeleteProductCommand> handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new DeleteProductCommand(id), ct);
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );
    }
}
