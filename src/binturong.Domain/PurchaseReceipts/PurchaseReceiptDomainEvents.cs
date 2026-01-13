using SharedKernel;

namespace Domain.PurchaseReceipts;

public sealed record PurchaseReceiptCreatedDomainEvent(Guid ReceiptId) : IDomainEvent;
