using SharedKernel;

namespace Domain.PurchaseReceiptDetails;

public sealed record PurchaseReceiptDetailCreatedDomainEvent(Guid ReceiptDetailId) : IDomainEvent;
