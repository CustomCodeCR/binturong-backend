using Domain.ShoppingCarts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.ECommerce;

internal sealed class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
{
    public void Configure(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.ToTable("ShoppingCarts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("CartId");

        builder.Property(x => x.WebClientId).HasColumnName("WebClientId");
        builder.Property(x => x.Status).HasMaxLength(20);

        builder.HasIndex(x => x.WebClientId);

        builder
            .HasOne(x => x.WebClient)
            .WithMany(x => x.ShoppingCarts)
            .HasForeignKey(x => x.WebClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Items)
            .WithOne(x => x.Cart)
            .HasForeignKey(x => x.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
