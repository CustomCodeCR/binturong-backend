namespace Api.Endpoints.Clients;

public sealed record AddClientContactRequest(
    string Name,
    string? JobTitle,
    string Email,
    string? Phone,
    bool IsPrimary
);

public sealed record UpdateClientContactRequest(
    string Name,
    string? JobTitle,
    string Email,
    string? Phone,
    bool IsPrimary
);
