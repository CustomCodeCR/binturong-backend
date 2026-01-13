using SharedKernel;

namespace Domain.PurchaseReceiptDetails;

public sealed class PurchaseReceiptDetail : Entity
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public Guid ProductId { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }

    public Domain.PurchaseReceipts.PurchaseReceipt? Receipt { get; set; }
    public Domain.Products.Product? Product { get; set; }
}
