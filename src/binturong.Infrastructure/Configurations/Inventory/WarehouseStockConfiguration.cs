using Domain.WarehouseStocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Inventory;

internal sealed class WarehouseStockConfiguration : IEntityTypeConfiguration<WarehouseStock>
{
    public void Configure(EntityTypeBuilder<WarehouseStock> builder)
    {
        builder.ToTable("WarehouseStock");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("StockId");

        builder.Property(x => x.ProductId).HasColumnName("ProductId");
        builder.Property(x => x.WarehouseId).HasColumnName("WarehouseId");

        builder.Property(x => x.CurrentStock).HasPrecision(18, 2);
        builder.Property(x => x.MinStock).HasPrecision(18, 2);
        builder.Property(x => x.MaxStock).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.ProductId, x.WarehouseId }).IsUnique();

        builder
            .HasOne(x => x.Product)
            .WithMany(x => x.WarehouseStocks)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Warehouse)
            .WithMany(x => x.WarehouseStocks)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
