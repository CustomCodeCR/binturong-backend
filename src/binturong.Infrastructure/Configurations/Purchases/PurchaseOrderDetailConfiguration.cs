using Domain.PurchaseOrderDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Purchases;

internal sealed class PurchaseOrderDetailConfiguration
    : IEntityTypeConfiguration<PurchaseOrderDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderDetail> builder)
    {
        builder.ToTable("PurchaseOrderDetails");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("PurchaseOrderDetailId");

        builder.Property(x => x.PurchaseOrderId).HasColumnName("PurchaseOrderId");
        builder.Property(x => x.ProductId).HasColumnName("ProductId");

        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.DiscountPerc).HasPrecision(10, 4);
        builder.Property(x => x.TaxPerc).HasPrecision(10, 4);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);

        builder.HasIndex(x => x.PurchaseOrderId);

        builder
            .HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Product)
            .WithMany(x => x.PurchaseOrderDetails)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
