namespace Api.Endpoints.Suppliers;

public sealed record CreateSupplierEvaluationRequest(
    Guid SupplierId,
    int Score,
    string Comment,
    DateTime EvaluatedAtUtc
);
