using Domain.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Warehouses;

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("WarehouseId");

        builder.Property(x => x.BranchId).HasColumnName("BranchId");

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);

        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => new { x.BranchId, x.Code });

        builder
            .HasOne(x => x.Branch)
            .WithMany(x => x.Warehouses)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.WarehouseStocks)
            .WithOne(x => x.Warehouse)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Two FKs to the same principal (Warehouse)
        builder
            .HasMany(x => x.MovementsFrom)
            .WithOne(x => x.FromWarehouse)
            .HasForeignKey(x => x.WarehouseFrom)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.MovementsTo)
            .WithOne(x => x.ToWarehouse)
            .HasForeignKey(x => x.WarehouseTo)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.PurchaseReceipts)
            .WithOne(x => x.Warehouse)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
