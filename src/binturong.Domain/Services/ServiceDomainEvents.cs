using SharedKernel;

namespace Domain.Services;

public sealed record ServiceCreatedDomainEvent(
    Guid ServiceId,
    string Code,
    string Name,
    string Description,
    Guid CategoryId,
    string CategoryName,
    bool IsCategoryProtected,
    int StandardTimeMin,
    decimal BaseRate,
    bool IsActive,
    string AvailabilityStatus
) : IDomainEvent;

public sealed record ServiceUpdatedDomainEvent(
    Guid ServiceId,
    string Code,
    string Name,
    string Description,
    Guid CategoryId,
    string CategoryName,
    bool IsCategoryProtected,
    int StandardTimeMin,
    decimal BaseRate,
    bool IsActive,
    string AvailabilityStatus,
    DateTime UpdatedAtUtc
) : IDomainEvent;

public sealed record ServiceDeletedDomainEvent(Guid ServiceId) : IDomainEvent;
