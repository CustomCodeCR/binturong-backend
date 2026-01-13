using Domain.SalesOrderDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.SalesOrders;

internal sealed class SalesOrderDetailConfiguration : IEntityTypeConfiguration<SalesOrderDetail>
{
    public void Configure(EntityTypeBuilder<SalesOrderDetail> builder)
    {
        builder.ToTable("SalesOrderDetails");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("SalesOrderDetailId");

        builder.Property(x => x.SalesOrderId).HasColumnName("SalesOrderId");
        builder.Property(x => x.ProductId).HasColumnName("ProductId");

        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.DiscountPerc).HasPrecision(10, 4);
        builder.Property(x => x.TaxPerc).HasPrecision(10, 4);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);

        builder.HasIndex(x => x.SalesOrderId);

        builder
            .HasOne(x => x.SalesOrder)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Product)
            .WithMany(x => x.SalesOrderDetails)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
