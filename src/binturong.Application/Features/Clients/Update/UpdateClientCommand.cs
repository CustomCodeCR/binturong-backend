using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Update;

public sealed record UpdateClientCommand(
    Guid ClientId,
    string TradeName,
    string ContactName,
    string Email,
    string PrimaryPhone,
    string? SecondaryPhone,
    string? Industry,
    string? ClientType,
    int Score,
    bool IsActive
) : ICommand;
