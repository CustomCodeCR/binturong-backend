using SharedKernel;

namespace Domain.WarehouseStocks;

public sealed class WarehouseStock : Entity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal MinStock { get; set; }
    public decimal MaxStock { get; set; }

    public Domain.Products.Product? Product { get; set; }
    public Domain.Warehouses.Warehouse? Warehouse { get; set; }
}
