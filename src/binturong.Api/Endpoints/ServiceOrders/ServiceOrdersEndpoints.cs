using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.ServiceOrders.AssignTechnician;
using Application.Features.ServiceOrders.Create;
using Application.Features.ServiceOrders.GetServiceOrderById;
using Application.Features.ServiceOrders.GetServiceOrders;
using Application.ReadModels.Services;
using Application.Security.Scopes;

namespace Api.Endpoints.ServiceOrders;

public sealed class ServiceOrdersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/service-orders").WithTags("ServiceOrders");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    string? status,
                    Guid? contractId,
                    IQueryHandler<
                        GetServiceOrdersQuery,
                        IReadOnlyList<ServiceOrderReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetServiceOrdersQuery(
                            page ?? 1,
                            pageSize ?? 50,
                            search,
                            status,
                            contractId
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ServiceOrdersRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetServiceOrderByIdQuery, ServiceOrderReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetServiceOrderByIdQuery(id), ct);

                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ServiceOrdersRead);

        group
            .MapPost(
                "/",
                async (
                    CreateServiceOrderRequest req,
                    ICommandHandler<CreateServiceOrderCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var services = (req.Services ?? [])
                        .Select(x => new CreateServiceOrderServiceItem(x.ServiceId, x.Quantity))
                        .ToArray();

                    var materials = (req.Materials ?? [])
                        .Select(x => new CreateServiceOrderMaterialItem(
                            x.ProductId,
                            x.Quantity,
                            x.EstimatedCost
                        ))
                        .ToArray();

                    var checklists = (req.Checklists ?? [])
                        .Select(x => new CreateServiceOrderChecklistItem(
                            x.Description,
                            x.IsCompleted
                        ))
                        .ToArray();

                    var photos = (req.Photos ?? [])
                        .Select(x => new CreateServiceOrderPhotoItem(x.PhotoS3Key, x.Description))
                        .ToArray();

                    var result = await handler.Handle(
                        new CreateServiceOrderCommand(
                            req.Code,
                            req.ClientId,
                            req.BranchId,
                            req.ContractId,
                            req.ScheduledDate,
                            req.ServiceAddress,
                            req.Notes,
                            services,
                            materials,
                            checklists,
                            photos
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/service-orders/{result.Value}",
                            new { serviceOrderId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.ServiceOrdersCreate);

        group
            .MapPost(
                "/{id:guid}/assign-technician",
                async (
                    Guid id,
                    AssignServiceOrderTechnicianRequest req,
                    ICommandHandler<AssignServiceOrderTechnicianCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new AssignServiceOrderTechnicianCommand(id, req.EmployeeId, req.TechRole),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ServiceOrdersAssignTechnician);
    }
}
