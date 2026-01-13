using Domain.AccountsPayable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Payables;

internal sealed class AccountPayableConfiguration : IEntityTypeConfiguration<AccountPayable>
{
    public void Configure(EntityTypeBuilder<AccountPayable> builder)
    {
        builder.ToTable("AccountsPayable");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("AccountPayableId");

        builder.Property(x => x.SupplierId).HasColumnName("SupplierId");
        builder.Property(x => x.PurchaseOrderId).HasColumnName("PurchaseOrderId");

        builder.Property(x => x.SupplierInvoiceId).HasMaxLength(100);

        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.PendingBalance).HasPrecision(18, 2);

        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.Status).HasMaxLength(20);

        builder.HasIndex(x => x.SupplierId);

        builder
            .HasOne(x => x.Supplier)
            .WithMany(x => x.AccountsPayables)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.AccountsPayables)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
