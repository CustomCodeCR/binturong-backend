using Application.Abstractions.Messaging;
using Application.ReadModels.Purchases;

namespace Application.Features.Purchases.PurchaseOrders.GetPurchaseOrderById;

public sealed record GetPurchaseOrderByIdQuery(Guid PurchaseOrderId)
    : IQuery<PurchaseOrderReadModel>;
