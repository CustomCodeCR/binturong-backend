using Domain.AuditLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Audit;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("AuditId");

        builder.Property(x => x.UserId).HasColumnName("UserId");

        builder.Property(x => x.Module).HasMaxLength(100);

        // Column in DB is "Entity" (reserved/awkward with your base Entity)
        builder.Property(x => x.EntityName).HasColumnName("Entity").HasMaxLength(100);

        builder.Property(x => x.Action).HasMaxLength(50);
        builder.Property(x => x.IP).HasMaxLength(50);
        builder.Property(x => x.UserAgent).HasMaxLength(255);

        builder.Property(x => x.DataBefore).HasColumnType("text");
        builder.Property(x => x.DataAfter).HasColumnType("text");

        builder.HasIndex(x => x.EventDate);
        builder.HasIndex(x => x.UserId);

        builder
            .HasOne(x => x.User)
            .WithMany(x => x.AuditLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
