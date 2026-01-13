using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Suppliers;

internal sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("SupplierId");

        builder.Property(x => x.IdentificationType).HasMaxLength(20);
        builder.Property(x => x.Identification).HasMaxLength(30);

        builder.Property(x => x.LegalName).HasMaxLength(150);
        builder.Property(x => x.TradeName).HasMaxLength(150);

        builder.Property(x => x.Email).HasMaxLength(150);
        builder.Property(x => x.Phone).HasMaxLength(50);

        builder.Property(x => x.PaymentTerms).HasMaxLength(100);
        builder.Property(x => x.MainCurrency).HasMaxLength(10);

        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => new { x.IdentificationType, x.Identification });

        builder
            .HasMany(x => x.Contacts)
            .WithOne(x => x.Supplier)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Attachments)
            .WithOne(x => x.Supplier)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.PurchaseOrders)
            .WithOne(x => x.Supplier)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
