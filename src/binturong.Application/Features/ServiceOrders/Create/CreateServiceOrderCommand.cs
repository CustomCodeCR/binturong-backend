using Application.Abstractions.Messaging;

namespace Application.Features.ServiceOrders.Create;

public sealed record CreateServiceOrderCommand(
    string Code,
    Guid ClientId,
    Guid? BranchId,
    Guid? ContractId,
    DateTime ScheduledDate,
    string ServiceAddress,
    string Notes,
    IReadOnlyList<CreateServiceOrderServiceItem> Services,
    IReadOnlyList<CreateServiceOrderMaterialItem> Materials,
    IReadOnlyList<CreateServiceOrderChecklistItem> Checklists,
    IReadOnlyList<CreateServiceOrderPhotoItem> Photos
) : ICommand<Guid>;

public sealed record CreateServiceOrderServiceItem(Guid ServiceId, decimal Quantity);

public sealed record CreateServiceOrderMaterialItem(
    Guid ProductId,
    decimal Quantity,
    decimal EstimatedCost
);

public sealed record CreateServiceOrderChecklistItem(string Description, bool IsCompleted);

public sealed record CreateServiceOrderPhotoItem(string PhotoS3Key, string Description);
