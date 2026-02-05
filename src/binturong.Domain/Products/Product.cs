using SharedKernel;

namespace Domain.Products;

public sealed class Product : Entity
{
    public Guid Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }
    public Guid? UomId { get; set; }
    public Guid? TaxId { get; set; }

    public decimal BasePrice { get; set; }
    public decimal AverageCost { get; set; }
    public bool IsService { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Domain.ProductCategories.ProductCategory? Category { get; set; }
    public Domain.UnitsOfMeasure.UnitOfMeasure? Uom { get; set; }
    public Domain.Taxes.Tax? Tax { get; set; }

    public ICollection<Domain.WarehouseStocks.WarehouseStock> WarehouseStocks { get; set; } =
        new List<Domain.WarehouseStocks.WarehouseStock>();
    public ICollection<Domain.InventoryMovements.InventoryMovement> InventoryMovements { get; set; } =
        new List<Domain.InventoryMovements.InventoryMovement>();

    public ICollection<Domain.QuoteDetails.QuoteDetail> QuoteDetails { get; set; } =
        new List<Domain.QuoteDetails.QuoteDetail>();
    public ICollection<Domain.SalesOrderDetails.SalesOrderDetail> SalesOrderDetails { get; set; } =
        new List<Domain.SalesOrderDetails.SalesOrderDetail>();
    public ICollection<Domain.InvoiceDetails.InvoiceDetail> InvoiceDetails { get; set; } =
        new List<Domain.InvoiceDetails.InvoiceDetail>();

    public ICollection<Domain.PurchaseOrderDetails.PurchaseOrderDetail> PurchaseOrderDetails { get; set; } =
        new List<Domain.PurchaseOrderDetails.PurchaseOrderDetail>();
    public ICollection<Domain.PurchaseReceiptDetails.PurchaseReceiptDetail> PurchaseReceiptDetails { get; set; } =
        new List<Domain.PurchaseReceiptDetails.PurchaseReceiptDetail>();

    public ICollection<Domain.ServiceOrderMaterials.ServiceOrderMaterial> ServiceOrderMaterials { get; set; } =
        new List<Domain.ServiceOrderMaterials.ServiceOrderMaterial>();
    public ICollection<Domain.CartItems.CartItem> CartItems { get; set; } =
        new List<Domain.CartItems.CartItem>();

    public void RaiseCreated() =>
        Raise(
            new ProductCreatedDomainEvent(
                Id,
                SKU,
                string.IsNullOrWhiteSpace(Barcode) ? null : Barcode,
                Name,
                string.IsNullOrWhiteSpace(Description) ? null : Description,
                CategoryId,
                UomId,
                TaxId,
                BasePrice,
                AverageCost,
                IsService,
                IsActive,
                CreatedAt,
                UpdatedAt
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new ProductUpdatedDomainEvent(
                Id,
                SKU,
                string.IsNullOrWhiteSpace(Barcode) ? null : Barcode,
                Name,
                string.IsNullOrWhiteSpace(Description) ? null : Description,
                CategoryId,
                UomId,
                TaxId,
                BasePrice,
                AverageCost,
                IsService,
                IsActive,
                UpdatedAt
            )
        );

    public void RaiseDeleted() => Raise(new ProductDeletedDomainEvent(Id));
}
