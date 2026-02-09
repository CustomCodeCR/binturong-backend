using Application.Abstractions.Messaging;
using Application.ReadModels.Purchases;

namespace Application.Features.Purchases.PurchaseReceipts.GetPurchaseReceiptById;

public sealed record GetPurchaseReceiptByIdQuery(Guid ReceiptId) : IQuery<PurchaseReceiptReadModel>;
