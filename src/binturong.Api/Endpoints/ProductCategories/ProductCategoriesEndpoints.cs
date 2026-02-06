using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.ProductCategories.Create;
using Application.Features.ProductCategories.Delete;
using Application.Features.ProductCategories.GetProductCategories;
using Application.Features.ProductCategories.GetProductCategoryById;
using Application.Features.ProductCategories.Update;
using Application.ReadModels.MasterData;
using Application.Security.Scopes;

namespace Api.Endpoints.ProductCategories;

public sealed class ProductCategoriesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/product-categories").WithTags("ProductCategories");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<
                        GetProductCategoriesQuery,
                        IReadOnlyList<ProductCategoryReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetProductCategoriesQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.CategoriesRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetProductCategoryByIdQuery, ProductCategoryReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetProductCategoryByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.CategoriesRead);

        group
            .MapPost(
                "/",
                async (
                    CreateProductCategoryRequest req,
                    ICommandHandler<CreateProductCategoryCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreateProductCategoryCommand(req.Name, req.Description, req.IsActive),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/product-categories/{result.Value}",
                            new { categoryId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.CategoriesCreate);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UpdateProductCategoryRequest req,
                    ICommandHandler<UpdateProductCategoryCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new UpdateProductCategoryCommand(
                            id,
                            req.Name,
                            req.Description,
                            req.IsActive
                        ),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.CategoriesUpdate);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ICommandHandler<DeleteProductCategoryCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new DeleteProductCategoryCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.CategoriesDelete);
    }
}
