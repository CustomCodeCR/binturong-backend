namespace Api.Endpoints.Suppliers;

public sealed record AddSupplierContactRequest(
    string Name,
    string? JobTitle,
    string Email,
    string? Phone,
    bool IsPrimary
);

public sealed record UpdateSupplierContactRequest(
    string Name,
    string? JobTitle,
    string Email,
    string? Phone,
    bool IsPrimary
);
