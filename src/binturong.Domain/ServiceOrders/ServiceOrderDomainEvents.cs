using SharedKernel;

namespace Domain.ServiceOrders;

public sealed record ServiceOrderCreatedDomainEvent(
    Guid ServiceOrderId,
    string Code,
    Guid ClientId,
    Guid? BranchId,
    Guid? ContractId,
    DateTime ScheduledDateUtc,
    string Status,
    string ServiceAddress,
    string Notes
) : IDomainEvent;

public sealed record ServiceOrderUpdatedDomainEvent(
    Guid ServiceOrderId,
    DateTime ScheduledDateUtc,
    string Status,
    string ServiceAddress,
    string Notes,
    DateTime UpdatedAtUtc
) : IDomainEvent;

public sealed record ServiceOrderServiceAddedDomainEvent(
    Guid ServiceOrderId,
    Guid ServiceOrderServiceId,
    Guid ServiceId,
    string ServiceName,
    decimal Quantity,
    decimal RateApplied,
    decimal LineTotal
) : IDomainEvent;

public sealed record ServiceOrderTechnicianAssignedDomainEvent(
    Guid ServiceOrderId,
    Guid ServiceOrderTechnicianId,
    Guid EmployeeId,
    string EmployeeName,
    string TechRole,
    DateTime AssignedAtUtc
) : IDomainEvent;

public sealed record ServiceOrderMaterialAddedDomainEvent(
    Guid ServiceOrderId,
    Guid ServiceOrderMaterialId,
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal EstimatedCost
) : IDomainEvent;

public sealed record ServiceOrderChecklistAddedDomainEvent(
    Guid ServiceOrderId,
    Guid ChecklistId,
    string Description,
    bool IsCompleted
) : IDomainEvent;

public sealed record ServiceOrderPhotoAddedDomainEvent(
    Guid ServiceOrderId,
    Guid PhotoId,
    string PhotoS3Key,
    string Description
) : IDomainEvent;

public sealed record ServiceOrderClosedDomainEvent(Guid ServiceOrderId, DateTime ClosedDateUtc)
    : IDomainEvent;

public sealed record ServiceOrderDeletedDomainEvent(Guid ServiceOrderId) : IDomainEvent;
