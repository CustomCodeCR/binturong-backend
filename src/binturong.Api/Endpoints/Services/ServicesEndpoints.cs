using Api.Security;
using Application.Abstractions.Messaging;
using Application.Common.Selects;
using Application.Features.Services.Create;
using Application.Features.Services.GetServiceById;
using Application.Features.Services.GetServices;
using Application.Features.Services.GetServicesSelect;
using Application.Features.Services.Update;
using Application.ReadModels.Services;
using Application.Security.Scopes;

namespace Api.Endpoints.Services;

public sealed class ServicesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/services").WithTags("Services");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    Guid? categoryId,
                    bool? isActive,
                    IQueryHandler<GetServicesQuery, IReadOnlyList<ServiceReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetServicesQuery(
                            page ?? 1,
                            pageSize ?? 50,
                            search,
                            categoryId,
                            isActive
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ServicesRead);

        group
            .MapGet(
                "/select",
                async (
                    string? search,
                    IQueryHandler<GetServicesSelectQuery, IReadOnlyList<SelectOptionDto>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetServicesSelectQuery(search), ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ServicesRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetServiceByIdQuery, ServiceReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetServiceByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ServicesRead);

        group
            .MapPost(
                "/",
                async (
                    CreateServiceRequest req,
                    ICommandHandler<CreateServiceCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreateServiceCommand(
                            req.Code,
                            req.Name,
                            req.Description,
                            req.CategoryId,
                            req.IsCategoryProtected,
                            req.StandardTimeMin,
                            req.BaseRate,
                            req.IsActive,
                            req.AvailabilityStatus
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/services/{result.Value}",
                            new { serviceId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.ServicesCreate);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UpdateServiceRequest req,
                    ICommandHandler<UpdateServiceCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new UpdateServiceCommand(
                            id,
                            req.Code,
                            req.Name,
                            req.Description,
                            req.CategoryId,
                            req.IsCategoryProtected,
                            req.StandardTimeMin,
                            req.BaseRate,
                            req.IsActive,
                            req.AvailabilityStatus
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ServicesUpdate);
    }
}
