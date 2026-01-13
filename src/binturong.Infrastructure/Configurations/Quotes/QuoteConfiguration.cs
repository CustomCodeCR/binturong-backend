using Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Quotes;

internal sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("QuoteId");

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();

        builder.Property(x => x.ClientId).HasColumnName("ClientId");
        builder.Property(x => x.BranchId).HasColumnName("BranchId");

        builder.Property(x => x.Status).HasMaxLength(20);
        builder.Property(x => x.Currency).HasMaxLength(10);

        builder.Property(x => x.ExchangeRate).HasPrecision(18, 4);
        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.Taxes).HasPrecision(18, 2);
        builder.Property(x => x.Discounts).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.Property(x => x.Notes).HasMaxLength(255);

        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.ClientId);

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.Quotes)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Branch)
            .WithMany(x => x.Quotes)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(x => x.Details)
            .WithOne(x => x.Quote)
            .HasForeignKey(x => x.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
