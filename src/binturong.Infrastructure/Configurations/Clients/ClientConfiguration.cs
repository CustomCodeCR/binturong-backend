using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Clients;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ClientId");

        builder.Property(x => x.PersonType).HasMaxLength(10).IsRequired();
        builder.Property(x => x.IdentificationType).HasMaxLength(20);
        builder.Property(x => x.Identification).HasMaxLength(30);

        builder.Property(x => x.TradeName).HasMaxLength(150);
        builder.Property(x => x.ContactName).HasMaxLength(150);
        builder.Property(x => x.Email).HasMaxLength(150);

        builder.Property(x => x.PrimaryPhone).HasMaxLength(50);
        builder.Property(x => x.SecondaryPhone).HasMaxLength(50);

        builder.Property(x => x.Industry).HasMaxLength(100);
        builder.Property(x => x.ClientType).HasMaxLength(50);

        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => new { x.IdentificationType, x.Identification });

        builder
            .HasMany(x => x.Addresses)
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Contacts)
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Attachments)
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.WebClients)
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
