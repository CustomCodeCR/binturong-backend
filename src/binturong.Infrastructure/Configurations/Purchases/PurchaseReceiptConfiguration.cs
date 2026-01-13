using Domain.PurchaseReceipts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Purchases;

internal sealed class PurchaseReceiptConfiguration : IEntityTypeConfiguration<PurchaseReceipt>
{
    public void Configure(EntityTypeBuilder<PurchaseReceipt> builder)
    {
        builder.ToTable("PurchaseReceipts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ReceiptId");

        builder.Property(x => x.PurchaseOrderId).HasColumnName("PurchaseOrderId");
        builder.Property(x => x.WarehouseId).HasColumnName("WarehouseId");

        builder.Property(x => x.Status).HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(255);

        builder.HasIndex(x => x.PurchaseOrderId);

        builder
            .HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.Receipts)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Warehouse)
            .WithMany(x => x.PurchaseReceipts)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.Details)
            .WithOne(x => x.Receipt)
            .HasForeignKey(x => x.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
