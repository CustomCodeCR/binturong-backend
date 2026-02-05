using Domain.InventoryMovementTypes;
using SharedKernel;

namespace Domain.InventoryMovements;

public sealed class InventoryMovement : Entity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? WarehouseFrom { get; set; }
    public Guid? WarehouseTo { get; set; }
    public InventoryMovementType MovementType { get; set; }
    public DateTime MovementDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string SourceModule { get; set; } = string.Empty;
    public int? SourceId { get; set; }
    public string Notes { get; set; } = string.Empty;

    public Domain.Products.Product? Product { get; set; }
    public Domain.Warehouses.Warehouse? FromWarehouse { get; set; }
    public Domain.Warehouses.Warehouse? ToWarehouse { get; set; }
}
