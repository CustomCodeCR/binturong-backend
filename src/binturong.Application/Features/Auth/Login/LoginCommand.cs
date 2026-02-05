using Application.Abstractions.Messaging;

namespace Application.Features.Auth.Login;

public sealed record LoginCommand(string UsernameOrEmail, string Password)
    : ICommand<LoginResponse>;
