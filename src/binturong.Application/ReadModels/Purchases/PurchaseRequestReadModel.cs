namespace Application.ReadModels.Purchases;

public sealed class PurchaseRequestReadModel
{
    public string Id { get; init; } = default!; // "purchase_request:{RequestId}"
    public int RequestId { get; init; }

    public string Code { get; init; } = default!;
    public int? BranchId { get; init; }
    public string? BranchName { get; init; }

    public int RequestedById { get; init; }
    public string RequestedByName { get; init; } = default!;

    public DateTime RequestDate { get; init; }
    public string Status { get; init; } = default!;
    public string? Notes { get; init; }
}
