namespace Application.ReadModels.Services;

public sealed class ServiceOrderReadModel
{
    public string Id { get; init; } = default!; // "service_order:{ServiceOrderId}"
    public Guid ServiceOrderId { get; init; }

    public string Code { get; init; } = default!;

    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = default!;

    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }

    public Guid? ContractId { get; init; }
    public string? ContractCode { get; init; }

    public DateTime ScheduledDate { get; init; }
    public DateTime? ClosedDate { get; init; }

    public string Status { get; init; } = default!;
    public string ServiceAddress { get; init; } = default!;
    public string? Notes { get; init; }

    public IReadOnlyList<ServiceOrderServiceLineReadModel> Services { get; init; } = [];
    public IReadOnlyList<ServiceOrderTechnicianLineReadModel> Technicians { get; init; } = [];
    public IReadOnlyList<ServiceOrderMaterialLineReadModel> Materials { get; init; } = [];
    public IReadOnlyList<ServiceOrderChecklistLineReadModel> Checklists { get; init; } = [];
    public IReadOnlyList<ServiceOrderPhotoLineReadModel> Photos { get; init; } = [];
}

public sealed class ServiceOrderServiceLineReadModel
{
    public Guid ServiceOrderServiceId { get; init; }
    public Guid ServiceId { get; init; }
    public string ServiceName { get; init; } = default!;
    public decimal Quantity { get; init; }
    public decimal RateApplied { get; init; }
    public decimal LineTotal { get; init; }
}

public sealed class ServiceOrderTechnicianLineReadModel
{
    public Guid ServiceOrderTechnicianId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = default!;
    public string TechRole { get; init; } = default!;
    public DateTime AssignedAtUtc { get; init; }
}

public sealed class ServiceOrderMaterialLineReadModel
{
    public Guid ServiceOrderMaterialId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public decimal Quantity { get; init; }
    public decimal EstimatedCost { get; init; }
}

public sealed class ServiceOrderChecklistLineReadModel
{
    public Guid ChecklistId { get; init; }
    public string Description { get; init; } = default!;
    public bool IsCompleted { get; init; }
}

public sealed class ServiceOrderPhotoLineReadModel
{
    public Guid PhotoId { get; init; }
    public string PhotoS3Key { get; init; } = default!;
    public string? Description { get; init; }
}
