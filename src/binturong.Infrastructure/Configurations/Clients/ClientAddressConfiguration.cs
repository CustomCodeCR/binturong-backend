using Domain.ClientAddresses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Clients;

internal sealed class ClientAddressConfiguration : IEntityTypeConfiguration<ClientAddress>
{
    public void Configure(EntityTypeBuilder<ClientAddress> builder)
    {
        builder.ToTable("ClientAddresses");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("AddressId");

        builder.Property(x => x.ClientId).HasColumnName("ClientId");

        builder.Property(x => x.AddressType).HasMaxLength(50);
        builder.Property(x => x.AddressLine).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Province).HasMaxLength(100);
        builder.Property(x => x.Canton).HasMaxLength(100);
        builder.Property(x => x.District).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(255);

        builder.HasIndex(x => x.ClientId);

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
