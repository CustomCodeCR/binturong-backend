using Domain.EmployeeHistory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Employees;

internal sealed class EmployeeHistoryConfiguration : IEntityTypeConfiguration<EmployeeHistoryEntry>
{
    public void Configure(EntityTypeBuilder<EmployeeHistoryEntry> builder)
    {
        builder.ToTable("EmployeeHistory");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("HistoryId");

        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeId");

        builder.Property(x => x.EventType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);

        builder.HasIndex(x => x.EmployeeId);

        builder
            .HasOne(x => x.Employee)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
