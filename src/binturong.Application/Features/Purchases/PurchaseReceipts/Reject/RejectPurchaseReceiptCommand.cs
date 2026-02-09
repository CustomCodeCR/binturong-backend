using Application.Abstractions.Messaging;

namespace Application.Features.Purchases.PurchaseReceipts.Reject;

public sealed record RejectPurchaseReceiptCommand(
    Guid ReceiptId,
    string Reason,
    DateTime RejectedAtUtc
) : ICommand;
