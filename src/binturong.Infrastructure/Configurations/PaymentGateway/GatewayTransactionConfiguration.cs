using Domain.GatewayTransactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.PaymentGateway;

internal sealed class GatewayTransactionConfiguration : IEntityTypeConfiguration<GatewayTransaction>
{
    public void Configure(EntityTypeBuilder<GatewayTransaction> builder)
    {
        builder.ToTable("GatewayTransactions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("GatewayTransactionId");

        builder.Property(x => x.GatewayId).HasColumnName("GatewayId");
        builder.Property(x => x.PaymentId).HasColumnName("PaymentId");
        builder.Property(x => x.InvoiceId).HasColumnName("InvoiceId");

        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.Status).HasMaxLength(20);
        builder.Property(x => x.AuthorizationCode).HasMaxLength(100);
        builder.Property(x => x.GatewayReference).HasMaxLength(100);

        builder.HasIndex(x => x.GatewayId);
        builder.HasIndex(x => x.TransactionDate);

        builder
            .HasOne(x => x.Gateway)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.GatewayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Payment)
            .WithMany(x => x.GatewayTransactions)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Invoice)
            .WithMany(x => x.GatewayTransactions)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
