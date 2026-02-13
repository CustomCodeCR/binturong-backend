using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.Invoices.GetInvoices;

public sealed record GetInvoicesQuery(int Page, int PageSize, string? Search)
    : IQuery<IReadOnlyList<InvoiceReadModel>>
{
    public int Skip => Math.Max(0, (Page - 1) * PageSize);
    public int Take => Math.Clamp(PageSize, 1, 200);
}
