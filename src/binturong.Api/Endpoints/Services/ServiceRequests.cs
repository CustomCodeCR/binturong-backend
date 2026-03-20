namespace Api.Endpoints.Services;

public sealed record CreateServiceRequest(
    string Code,
    string Name,
    string Description,
    Guid CategoryId,
    bool IsCategoryProtected,
    int StandardTimeMin,
    decimal BaseRate,
    bool IsActive,
    string AvailabilityStatus
);

public sealed record UpdateServiceRequest(
    string Code,
    string Name,
    string Description,
    Guid CategoryId,
    bool IsCategoryProtected,
    int StandardTimeMin,
    decimal BaseRate,
    bool IsActive,
    string AvailabilityStatus
);
