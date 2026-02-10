using Application.Abstractions.Messaging;
using Application.ReadModels.Purchases;

namespace Application.Features.SupplierQuotes.GetSupplierQuotes;

public sealed record GetSupplierQuotesQuery(
    string? Search,
    Guid? SupplierId,
    Guid? BranchId,
    string? Status,
    int Skip = 0,
    int Take = 50
) : IQuery<IReadOnlyList<SupplierQuoteReadModel>>;
