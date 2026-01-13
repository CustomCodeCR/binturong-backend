using Domain.QuoteDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Quotes;

internal sealed class QuoteDetailConfiguration : IEntityTypeConfiguration<QuoteDetail>
{
    public void Configure(EntityTypeBuilder<QuoteDetail> builder)
    {
        builder.ToTable("QuoteDetails");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("QuoteDetailId");

        builder.Property(x => x.QuoteId).HasColumnName("QuoteId");
        builder.Property(x => x.ProductId).HasColumnName("ProductId");

        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.DiscountPerc).HasPrecision(10, 4);
        builder.Property(x => x.TaxPerc).HasPrecision(10, 4);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);

        builder.HasIndex(x => x.QuoteId);

        builder
            .HasOne(x => x.Quote)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Product)
            .WithMany(x => x.QuoteDetails)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
