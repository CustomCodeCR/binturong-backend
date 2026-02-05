namespace Application.Features.Auth.Login;

public sealed record LoginResponse(
    Guid UserId,
    string Username,
    string Email,
    string Token,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Scopes
);
