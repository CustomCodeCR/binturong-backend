using Application.Abstractions.Messaging;
using Application.ReadModels.Purchases;

namespace Application.Features.Purchases.PurchaseRequests.GetPurchaseRequestById;

public sealed record GetPurchaseRequestByIdQuery(Guid RequestId) : IQuery<PurchaseRequestReadModel>;
