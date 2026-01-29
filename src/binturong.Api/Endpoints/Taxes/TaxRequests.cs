namespace Api.Endpoints.Taxes;

public sealed record CreateTaxRequest(
    string Name,
    string Code,
    decimal Percentage,
    bool IsActive = true
);

public sealed record UpdateTaxRequest(string Name, string Code, decimal Percentage, bool IsActive);
