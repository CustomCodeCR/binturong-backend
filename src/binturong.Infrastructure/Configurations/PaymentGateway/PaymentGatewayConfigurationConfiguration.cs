using Domain.PaymentGatewayConfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.PaymentGateway;

internal sealed class PaymentGatewayConfigurationConfiguration
    : IEntityTypeConfiguration<PaymentGatewayConfiguration>
{
    public void Configure(EntityTypeBuilder<PaymentGatewayConfiguration> builder)
    {
        builder.ToTable("PaymentGatewayConfig");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("GatewayId");

        builder.Property(x => x.Provider).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PublicKey).HasMaxLength(255);
        builder.Property(x => x.Environment).HasMaxLength(20);
        builder.Property(x => x.SecretRef).HasMaxLength(255);

        builder.HasIndex(x => new { x.Provider, x.Environment });

        builder
            .HasMany(x => x.Transactions)
            .WithOne(x => x.Gateway)
            .HasForeignKey(x => x.GatewayId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
