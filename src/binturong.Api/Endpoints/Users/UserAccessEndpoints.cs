using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Users.AssignRole;
using Application.Features.Users.RemoveRole;
using Application.Features.Users.SetUserScopes;
using Application.Security.Scopes;

namespace Api.Endpoints.Users;

public sealed class UserAccessEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group
            .MapPut(
                "/{id:guid}/role",
                async (
                    Guid id,
                    AssignUserRoleRequest req,
                    ICommandHandler<AssignRoleToUserCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new AssignRoleToUserCommand(id, req.RoleId, req.ReplaceExisting);
                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.UsersAssignRole);

        group
            .MapDelete(
                "/{id:guid}/role/{roleId:guid}",
                async (
                    Guid id,
                    Guid roleId,
                    ICommandHandler<RemoveRoleFromUserCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RemoveRoleFromUserCommand(id, roleId),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.UsersAssignRole);

        group
            .MapPut(
                "/{id:guid}/scopes",
                async (
                    Guid id,
                    SetUserScopesRequest req,
                    ICommandHandler<SetUserScopesCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new SetUserScopesCommand(id, req.ScopeIds);
                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.UsersAssignScopes);
    }
}
