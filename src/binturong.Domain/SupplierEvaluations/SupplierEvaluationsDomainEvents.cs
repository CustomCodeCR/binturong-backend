using SharedKernel;

namespace Domain.SupplierEvaluations;

public sealed record SupplierEvaluationCreatedDomainEvent(
    Guid SupplierEvaluationId,
    Guid SupplierId,
    int Score,
    string Classification, // Reliable | LowPerformance
    string Comment,
    DateTime EvaluatedAtUtc
) : IDomainEvent;
