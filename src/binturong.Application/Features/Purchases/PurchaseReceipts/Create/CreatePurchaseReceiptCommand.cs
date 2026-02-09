using Application.Abstractions.Messaging;

namespace Application.Features.Purchases.PurchaseReceipts.Create;

public sealed record CreatePurchaseReceiptCommand(
    Guid PurchaseOrderId,
    Guid WarehouseId,
    DateTime ReceiptDateUtc,
    string? Notes,
    IReadOnlyList<CreatePurchaseReceiptLineDto> Lines
) : ICommand<Guid>;

public sealed record CreatePurchaseReceiptLineDto(
    Guid ProductId,
    decimal QuantityReceived,
    decimal UnitCost
);
