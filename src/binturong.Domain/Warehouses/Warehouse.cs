using SharedKernel;

namespace Domain.Warehouses;

public sealed class Warehouse : Entity
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Domain.Branches.Branch? Branch { get; set; }

    public ICollection<Domain.WarehouseStocks.WarehouseStock> WarehouseStocks { get; set; } =
        new List<Domain.WarehouseStocks.WarehouseStock>();
    public ICollection<Domain.InventoryMovements.InventoryMovement> MovementsFrom { get; set; } =
        new List<Domain.InventoryMovements.InventoryMovement>();
    public ICollection<Domain.InventoryMovements.InventoryMovement> MovementsTo { get; set; } =
        new List<Domain.InventoryMovements.InventoryMovement>();

    public ICollection<Domain.PurchaseReceipts.PurchaseReceipt> PurchaseReceipts { get; set; } =
        new List<Domain.PurchaseReceipts.PurchaseReceipt>();
}
