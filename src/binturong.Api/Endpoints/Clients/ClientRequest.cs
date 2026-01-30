namespace Api.Endpoints.Clients;

public sealed record CreateClientRequest(
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
);

public sealed record UpdateClientRequest(
    string TradeName,
    string ContactName,
    string Email,
    string PrimaryPhone,
    string? SecondaryPhone,
    string? Industry,
    string? ClientType,
    int Score,
    bool IsActive
);
