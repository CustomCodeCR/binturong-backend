using Domain.InventoryMovementTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Inventory;

internal sealed class InventoryMovementTypeConfiguration
    : IEntityTypeConfiguration<InventoryMovementType>
{
    public void Configure(EntityTypeBuilder<InventoryMovementType> builder)
    {
        builder.ToTable("InventoryMovementTypes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("MovementTypeId");

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
