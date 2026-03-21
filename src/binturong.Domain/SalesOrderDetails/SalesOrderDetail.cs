using SharedKernel;

namespace Domain.SalesOrderDetails;

public sealed class SalesOrderDetail : Entity
{
    public Guid Id { get; set; }
    public Guid SalesOrderId { get; set; }

    public string ItemType { get; set; } = string.Empty; // Product | Service
    public Guid? ProductId { get; set; }
    public Guid? ServiceId { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    // Discount applied to this line
    public decimal DiscountPerc { get; set; }
    public decimal DiscountAmount { get; set; }
    public string DiscountReason { get; set; } = string.Empty;

    public decimal TaxPerc { get; set; }
    public decimal LineTotal { get; set; }

    public Domain.SalesOrders.SalesOrder? SalesOrder { get; set; }
    public Domain.Products.Product? Product { get; set; }
    public Domain.Services.Service? Service { get; set; }

    public decimal GetBaseAmount() => Quantity * UnitPrice;

    public Result ApplyDiscount(decimal discountPerc, string reason)
    {
        if (discountPerc < 0 || discountPerc > 100)
            return Result.Failure(Domain.Discounts.DiscountErrors.PercentageInvalid);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(Domain.Discounts.DiscountErrors.ReasonRequired);

        var baseAmount = GetBaseAmount();
        DiscountPerc = discountPerc;
        DiscountAmount = baseAmount * (discountPerc / 100m);
        DiscountReason = reason.Trim();

        RecalculateTotal();
        return Result.Success();
    }

    public void RemoveDiscount()
    {
        DiscountPerc = 0m;
        DiscountAmount = 0m;
        DiscountReason = string.Empty;
        RecalculateTotal();
    }

    public void RecalculateTotal()
    {
        var baseAmount = GetBaseAmount();
        var discountedBase = baseAmount - DiscountAmount;
        var taxAmount = discountedBase * (TaxPerc / 100m);
        LineTotal = discountedBase + taxAmount;
    }
}
