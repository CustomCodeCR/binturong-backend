using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Create;

public sealed record CreateClientCommand(
    string PersonType,
    string IdentificationType,
    string Identification,
    string TradeName,
    string ContactName,
    string Email,
    string PrimaryPhone,
    string? SecondaryPhone,
    string? Industry,
    string? ClientType,
    int Score = 0,
    bool IsActive = true
) : ICommand<Guid>;
