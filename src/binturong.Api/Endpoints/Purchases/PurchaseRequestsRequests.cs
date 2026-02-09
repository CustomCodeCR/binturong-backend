namespace Api.Endpoints.Purchases;

public sealed record CreatePurchaseRequestRequest(
    string Code,
    Guid? BranchId,
    Guid? RequestedById,
    DateTime RequestDateUtc,
    string? Notes
);
