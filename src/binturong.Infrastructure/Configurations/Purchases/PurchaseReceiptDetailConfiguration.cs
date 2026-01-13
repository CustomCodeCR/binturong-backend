using Domain.PurchaseReceiptDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Purchases;

internal sealed class PurchaseReceiptDetailConfiguration
    : IEntityTypeConfiguration<PurchaseReceiptDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseReceiptDetail> builder)
    {
        builder.ToTable("PurchaseReceiptDetails");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ReceiptDetailId");

        builder.Property(x => x.ReceiptId).HasColumnName("ReceiptId");
        builder.Property(x => x.ProductId).HasColumnName("ProductId");

        builder.Property(x => x.QuantityReceived).HasPrecision(18, 2);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);

        builder.HasIndex(x => x.ReceiptId);

        builder
            .HasOne(x => x.Receipt)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Product)
            .WithMany(x => x.PurchaseReceiptDetails)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
