using Domain.InventoryMovements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Inventory;

internal sealed class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("InventoryMovements");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("MovementId");

        builder.Property(x => x.ProductId).HasColumnName("ProductId");
        builder.Property(x => x.WarehouseFrom).HasColumnName("WarehouseFrom");
        builder.Property(x => x.WarehouseTo).HasColumnName("WarehouseTo");
        builder.Property(x => x.MovementTypeId).HasColumnName("MovementTypeId");

        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);

        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(255);

        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.MovementTypeId);
        builder.HasIndex(x => x.MovementDate);

        builder
            .HasOne(x => x.Product)
            .WithMany(x => x.InventoryMovements)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.MovementType)
            .WithMany(x => x.Movements)
            .HasForeignKey(x => x.MovementTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.FromWarehouse)
            .WithMany(x => x.MovementsFrom)
            .HasForeignKey(x => x.WarehouseFrom)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ToWarehouse)
            .WithMany(x => x.MovementsTo)
            .HasForeignKey(x => x.WarehouseTo)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
