namespace Application.ReadModels.CRM;

public sealed class SupplierEvaluationReadModel
{
    public string Id { get; init; } = default!; // "supplier_eval:{SupplierEvaluationId}"
    public Guid SupplierEvaluationId { get; init; }

    public Guid SupplierId { get; init; }
    public string SupplierName { get; init; } = default!;

    public int Score { get; init; }
    public string Classification { get; init; } = default!; // Reliable | LowPerformance

    public string Comment { get; init; } = default!;
    public DateTime EvaluatedAtUtc { get; init; }
}
