using Application.Abstractions.Messaging;
using Application.ReadModels.CRM;

namespace Application.Features.SupplierEvaluations.GetSupplierEvaluations;

public sealed record GetSupplierEvaluationsQuery(Guid SupplierId, int Page, int PageSize)
    : IQuery<IReadOnlyList<SupplierEvaluationReadModel>>
{
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}
