using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Accounting;

internal sealed class AccountingPeriodConfiguration : IEntityTypeConfiguration<AccountingPeriod>
{
    public void Configure(EntityTypeBuilder<AccountingPeriod> builder)
    {
        builder.ToTable("AccountingPeriods");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("PeriodId");

        builder.Property(x => x.Status).HasMaxLength(20);

        builder.HasIndex(x => new { x.Year, x.Month }).IsUnique();

        builder
            .HasMany(x => x.JournalEntries)
            .WithOne(x => x.Period)
            .HasForeignKey(x => x.PeriodId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
