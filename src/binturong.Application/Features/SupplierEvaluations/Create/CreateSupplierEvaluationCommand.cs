using Application.Abstractions.Messaging;

namespace Application.Features.SupplierEvaluations.Create;

public sealed record CreateSupplierEvaluationCommand(
    Guid SupplierId,
    int Score,
    string Comment,
    DateTime EvaluatedAtUtc
) : ICommand<Guid>;
