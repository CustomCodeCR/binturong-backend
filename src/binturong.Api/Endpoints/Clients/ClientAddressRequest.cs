namespace Api.Endpoints.Clients;

public sealed record AddClientAddressRequest(
    string AddressType,
    string AddressLine,
    string Province,
    string Canton,
    string District,
    string? Notes,
    bool IsPrimary
);

public sealed record UpdateClientAddressRequest(
    string AddressType,
    string AddressLine,
    string Province,
    string Canton,
    string District,
    string? Notes,
    bool IsPrimary
);
