using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Roles;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("RoleId");

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();

        builder.Property(x => x.Description).HasMaxLength(255);

        builder.Property(x => x.IsActive).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();

        builder
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.RoleScopes)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
