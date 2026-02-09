using Application.Abstractions.Messaging;

namespace Application.Features.Purchases.PurchaseRequests.Create;

public sealed record CreatePurchaseRequestCommand(
    string Code,
    Guid? BranchId,
    Guid? RequestedById,
    DateTime RequestDateUtc,
    string? Notes
) : ICommand<Guid>;
