using Domain.ClientContacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Clients;

internal sealed class ClientContactConfiguration : IEntityTypeConfiguration<ClientContact>
{
    public void Configure(EntityTypeBuilder<ClientContact> builder)
    {
        builder.ToTable("ClientContacts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ContactId");

        builder.Property(x => x.ClientId).HasColumnName("ClientId");

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.JobTitle).HasMaxLength(100);
        builder.Property(x => x.Email).HasMaxLength(150);
        builder.Property(x => x.Phone).HasMaxLength(50);

        builder.HasIndex(x => x.ClientId);

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
