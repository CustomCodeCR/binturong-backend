using SharedKernel;

namespace Domain.InventoryTransfers;

public sealed class InventoryTransferLine : Entity
{
    public Guid Id { get; set; }

    public Guid TransferId { get; set; }

    public Guid ProductId { get; set; }

    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }

    public decimal Quantity { get; set; }

    // Navigation
    public InventoryTransfer? Transfer { get; set; }

    public Domain.Products.Product? Product { get; set; }
    public Domain.Warehouses.Warehouse? FromWarehouse { get; set; }
    public Domain.Warehouses.Warehouse? ToWarehouse { get; set; }
}
