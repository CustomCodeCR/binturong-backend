using Application.Abstractions.Messaging;

namespace Application.Features.SalesOrders.GetSalesOrders;

public sealed record GetSalesOrdersQuery(int Page, int PageSize, string? Search)
    : IQuery<IReadOnlyList<Application.ReadModels.Sales.SalesOrderReadModel>>
{
    public int Skip => (Page <= 1 ? 0 : (Page - 1) * Take);
    public int Take => PageSize <= 0 ? 50 : PageSize;
}
