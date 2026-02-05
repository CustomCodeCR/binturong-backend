using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Roles.Create;
using Application.Features.Roles.Delete;
using Application.Features.Roles.GetRoleById;
using Application.Features.Roles.GetRoles;
using Application.Features.Roles.SetRoleScopes;
using Application.Features.Roles.Update;
using Application.ReadModels.Security;
using Application.Security;

namespace Api.Endpoints.Roles;

public sealed class RolesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles").WithTags("Roles");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<GetRolesQuery, IReadOnlyList<RoleReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var q = new GetRolesQuery(page ?? 1, pageSize ?? 50, search);
                    var result = await handler.Handle(q, ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.RolesRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetRoleByIdQuery, RoleReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetRoleByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.RolesRead);

        group
            .MapPost(
                "/",
                async (
                    CreateRoleRequest req,
                    ICommandHandler<CreateRoleCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new CreateRoleCommand(req.Name, req.Description, req.IsActive);
                    var result = await handler.Handle(cmd, ct);

                    if (result.IsFailure)
                        return Results.BadRequest(result.Error);

                    return Results.Created(
                        $"/api/roles/{result.Value}",
                        new { roleId = result.Value }
                    );
                }
            )
            .RequireScope(SecurityScopes.RolesCreate);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UpdateRoleRequest req,
                    ICommandHandler<UpdateRoleCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new UpdateRoleCommand(id, req.Name, req.Description, req.IsActive);
                    var result = await handler.Handle(cmd, ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.RolesUpdate);

        group
            .MapDelete(
                "/{id:guid}",
                async (Guid id, ICommandHandler<DeleteRoleCommand> handler, CancellationToken ct) =>
                {
                    var result = await handler.Handle(new DeleteRoleCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.RolesDelete);

        group
            .MapPut(
                "/{id:guid}/scopes",
                async (
                    Guid id,
                    SetRoleScopesRequest req,
                    ICommandHandler<SetRoleScopesCommand> handler,
                    Application.Abstractions.Security.ICurrentUser current,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new SetRoleScopesCommand(id, req.ScopeIds, current.UserId);
                    var result = await handler.Handle(cmd, ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.RolesAssignScopes);
    }
}
