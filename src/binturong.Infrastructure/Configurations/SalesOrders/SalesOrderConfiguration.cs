using Domain.SalesOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.SalesOrders;

internal sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("SalesOrderId");

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();

        builder.Property(x => x.QuoteId).HasColumnName("QuoteId");
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
            .HasOne(x => x.Quote)
            .WithMany(x => x.SalesOrders)
            .HasForeignKey(x => x.QuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.SalesOrders)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Branch)
            .WithMany(x => x.SalesOrders)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(x => x.Details)
            .WithOne(x => x.SalesOrder)
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
