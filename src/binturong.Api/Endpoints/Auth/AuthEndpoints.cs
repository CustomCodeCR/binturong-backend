using Application.Abstractions.Messaging;
using Application.Features.Auth.Login;

namespace Api.Endpoints.Auth;

public sealed class AuthEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost(
            "/login",
            async (
                LoginRequest req,
                ICommandHandler<LoginCommand, LoginResponse> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(
                    new LoginCommand(req.UsernameOrEmail, req.Password),
                    ct
                );

                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Ok(result.Value);
            }
        );
    }
}

public sealed record LoginRequest(string UsernameOrEmail, string Password);
