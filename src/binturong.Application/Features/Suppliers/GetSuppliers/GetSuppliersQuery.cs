using Application.Abstractions.Messaging;
using Application.ReadModels.CRM;

namespace Application.Features.Suppliers.GetSuppliers;

public sealed record GetSuppliersQuery(int Page = 1, int PageSize = 50, string? Search = null)
    : IQuery<IReadOnlyList<SupplierReadModel>>
{
    public int Take => PageSize;
    public int Skip => (Page - 1) * PageSize;
}
