using Application.Abstractions.Messaging;
using Application.ReadModels.Purchases;

namespace Application.Features.Suppliers.History.GetSupplierPurchaseHistory;

public sealed record GetSupplierPurchaseHistoryQuery(
    Guid SupplierId,
    DateTime? From,
    DateTime? To,
    string? Status,
    int Skip = 0,
    int Take = 50
) : IQuery<IReadOnlyList<PurchaseOrderReadModel>>;
