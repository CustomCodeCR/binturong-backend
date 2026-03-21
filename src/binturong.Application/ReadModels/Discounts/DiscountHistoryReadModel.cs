namespace Application.ReadModels.Discounts;

public sealed class DiscountHistoryReadModel
{
    public string Id { get; init; } = default!; // "discount_history:{HistoryId}"
    public Guid HistoryId { get; init; }

    public Guid SalesOrderId { get; init; }
    public string? SalesOrderCode { get; init; }

    public Guid? SalesOrderDetailId { get; init; }

    public string Scope { get; init; } = default!; // Line | Total
    public string Action { get; init; } = default!; // Applied | Removed | Requested | Approved | Rejected

    public decimal DiscountPercentage { get; init; }
    public decimal DiscountAmount { get; init; }

    public string? Reason { get; init; }
    public string? RejectionReason { get; init; }

    public Guid UserId { get; init; }
    public string? UserName { get; init; }

    public DateTime EventDateUtc { get; init; }
}
