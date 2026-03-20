namespace Api.Endpoints.ServiceOrders;

public sealed record CreateServiceOrderRequest(
    string Code,
    Guid ClientId,
    Guid? BranchId,
    Guid? ContractId,
    DateTime ScheduledDate,
    string ServiceAddress,
    string Notes,
    IReadOnlyList<CreateServiceOrderServiceRequest> Services,
    IReadOnlyList<CreateServiceOrderMaterialRequest> Materials,
    IReadOnlyList<CreateServiceOrderChecklistRequest> Checklists,
    IReadOnlyList<CreateServiceOrderPhotoRequest> Photos
);

public sealed record CreateServiceOrderServiceRequest(Guid ServiceId, decimal Quantity);

public sealed record CreateServiceOrderMaterialRequest(
    Guid ProductId,
    decimal Quantity,
    decimal EstimatedCost
);

public sealed record CreateServiceOrderChecklistRequest(string Description, bool IsCompleted);

public sealed record CreateServiceOrderPhotoRequest(string PhotoS3Key, string Description);

public sealed record AssignServiceOrderTechnicianRequest(Guid EmployeeId, string TechRole);
