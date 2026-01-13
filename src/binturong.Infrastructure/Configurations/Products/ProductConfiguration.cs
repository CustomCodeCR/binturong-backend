using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Products;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ProductId");

        builder.Property(x => x.SKU).HasMaxLength(50);
        builder.Property(x => x.Barcode).HasMaxLength(50);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);

        builder.Property(x => x.CategoryId).HasColumnName("CategoryId");
        builder.Property(x => x.UomId).HasColumnName("UomId");
        builder.Property(x => x.TaxId).HasColumnName("TaxId");

        builder.Property(x => x.BasePrice).HasPrecision(18, 2);
        builder.Property(x => x.AverageCost).HasPrecision(18, 4);

        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.SKU);
        builder.HasIndex(x => x.Barcode);

        builder
            .HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Uom)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.UomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Tax)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.TaxId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
