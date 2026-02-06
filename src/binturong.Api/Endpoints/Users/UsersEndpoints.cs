using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Users.Create;
using Application.Features.Users.Delete;
using Application.Features.Users.GetUserById;
using Application.Features.Users.GetUsers;
using Application.Features.Users.Update;
using Application.ReadModels.Security;
using Application.Security.Scopes;

namespace Api.Endpoints.Users;

public sealed class UsersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<GetUsersQuery, IReadOnlyList<UserReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var query = new GetUsersQuery(page ?? 1, pageSize ?? 50, search);
                    var result = await handler.Handle(query, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.UsersRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetUserByIdQuery, UserReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetUserByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.UsersRead);

        group
            .MapPost(
                "/",
                async (
                    CreateUserRequest req,
                    ICommandHandler<CreateUserCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new CreateUserCommand(
                        req.Username,
                        req.Email,
                        req.Password,
                        req.IsActive
                    );
                    var result = await handler.Handle(cmd, ct);

                    if (result.IsFailure)
                        return Results.BadRequest(result.Error);

                    return Results.Created(
                        $"/api/users/{result.Value}",
                        new { userId = result.Value }
                    );
                }
            )
            .RequireScope(SecurityScopes.UsersCreate);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UpdateUserRequest req,
                    ICommandHandler<UpdateUserCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new UpdateUserCommand(
                        id,
                        req.Username,
                        req.Email,
                        req.IsActive,
                        req.LastLogin,
                        req.MustChangePassword,
                        req.FailedAttempts,
                        req.LockedUntil
                    );

                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.UsersUpdate);

        group
            .MapDelete(
                "/{id:guid}",
                async (Guid id, ICommandHandler<DeleteUserCommand> handler, CancellationToken ct) =>
                {
                    var result = await handler.Handle(new DeleteUserCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.UsersDelete);
    }
}
