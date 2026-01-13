using Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Purchases;

internal sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("PurchaseOrderId");

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();

        builder.Property(x => x.SupplierId).HasColumnName("SupplierId");
        builder.Property(x => x.BranchId).HasColumnName("BranchId");
        builder.Property(x => x.RequestId).HasColumnName("RequestId");

        builder.Property(x => x.Status).HasMaxLength(20);
        builder.Property(x => x.Currency).HasMaxLength(10);

        builder.Property(x => x.ExchangeRate).HasPrecision(18, 4);
        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.Taxes).HasPrecision(18, 2);
        builder.Property(x => x.Discounts).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.SupplierId);

        builder
            .HasOne(x => x.Supplier)
            .WithMany(x => x.PurchaseOrders)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Branch)
            .WithMany(x => x.PurchaseOrders)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Request)
            .WithMany(x => x.PurchaseOrders)
            .HasForeignKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(x => x.Details)
            .WithOne(x => x.PurchaseOrder)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Receipts)
            .WithOne(x => x.PurchaseOrder)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.AccountsPayables)
            .WithOne(x => x.PurchaseOrder)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
