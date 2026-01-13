namespace Application.ReadModels.Services;

public sealed class ServiceOrderReadModel
{
    public string Id { get; init; } = default!; // "service_order:{ServiceOrderId}"
    public int ServiceOrderId { get; init; }

    public string Code { get; init; } = default!;
    public int ClientId { get; init; }
    public string ClientName { get; init; } = default!;

    public int? BranchId { get; init; }
    public string? BranchName { get; init; }

    public int? ContractId { get; init; }

    public DateTime ScheduledDate { get; init; }
    public DateTime? ClosedDate { get; init; }

    public string Status { get; init; } = default!;
    public string ServiceAddress { get; init; } = default!;
    public string? Notes { get; init; }

    public IReadOnlyList<ServiceOrderTechnicianReadModel> Technicians { get; init; } = [];
    public IReadOnlyList<ServiceOrderServiceLineReadModel> Services { get; init; } = [];
    public IReadOnlyList<ServiceOrderMaterialLineReadModel> Materials { get; init; } = [];
    public IReadOnlyList<ServiceOrderChecklistReadModel> Checklists { get; init; } = [];
    public IReadOnlyList<ServiceOrderPhotoReadModel> Photos { get; init; } = [];
}

public sealed class ServiceOrderTechnicianReadModel
{
    public int ServiceOrderTechId { get; init; }
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = default!;
    public string TechRole { get; init; } = default!;
}

public sealed class ServiceOrderServiceLineReadModel
{
    public int ServiceOrderServiceId { get; init; }
    public int ServiceId { get; init; }
    public string ServiceName { get; init; } = default!;

    public decimal Quantity { get; init; }
    public decimal RateApplied { get; init; }
    public decimal LineTotal { get; init; }
}

public sealed class ServiceOrderMaterialLineReadModel
{
    public int ServiceOrderMaterialId { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public decimal Quantity { get; init; }
    public decimal EstimatedCost { get; init; }
}

public sealed class ServiceOrderChecklistReadModel
{
    public int ChecklistId { get; init; }
    public string Description { get; init; } = default!;
    public bool IsCompleted { get; init; }
}

public sealed class ServiceOrderPhotoReadModel
{
    public int PhotoId { get; init; }
    public string PhotoS3Key { get; init; } = default!;
    public string? Description { get; init; }
}
