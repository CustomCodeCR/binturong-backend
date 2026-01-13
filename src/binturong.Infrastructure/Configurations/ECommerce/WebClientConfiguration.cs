using Domain.WebClients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.ECommerce;

internal sealed class WebClientConfiguration : IEntityTypeConfiguration<WebClient>
{
    public void Configure(EntityTypeBuilder<WebClient> builder)
    {
        builder.ToTable("WebClients");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("WebClientId");

        builder.Property(x => x.ClientId).HasColumnName("ClientId");

        builder.Property(x => x.LoginEmail).HasMaxLength(150).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();

        builder.HasIndex(x => x.LoginEmail).IsUnique();

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.WebClients)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(x => x.ShoppingCarts)
            .WithOne(x => x.WebClient)
            .HasForeignKey(x => x.WebClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.MarketingTrackings)
            .WithOne(x => x.WebClient)
            .HasForeignKey(x => x.WebClientId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
