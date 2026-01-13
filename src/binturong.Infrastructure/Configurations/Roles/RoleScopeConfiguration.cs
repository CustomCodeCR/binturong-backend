using Domain.RoleScopes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Roles;

internal sealed class RoleScopeConfiguration : IEntityTypeConfiguration<RoleScope>
{
    public void Configure(EntityTypeBuilder<RoleScope> builder)
    {
        builder.ToTable("RoleScopes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("RoleScopeId");

        builder.Property(x => x.RoleId).HasColumnName("RoleId");
        builder.Property(x => x.ScopeId).HasColumnName("ScopeId");

        builder.HasIndex(x => new { x.RoleId, x.ScopeId }).IsUnique();

        builder
            .HasOne(x => x.Role)
            .WithMany(x => x.RoleScopes)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Scope)
            .WithMany(x => x.RoleScopes)
            .HasForeignKey(x => x.ScopeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
