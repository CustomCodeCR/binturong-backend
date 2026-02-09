namespace Application.ReadModels.Purchases;

public sealed class PurchaseRequestReadModel
{
    public string Id { get; set; } = default!; // "purchase_request:{RequestId}"
    public Guid RequestId { get; set; }

    public string Code { get; set; } = default!;
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }

    public Guid? RequestedById { get; set; }
    public string RequestedByName { get; set; } = string.Empty;

    public DateTime RequestDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
