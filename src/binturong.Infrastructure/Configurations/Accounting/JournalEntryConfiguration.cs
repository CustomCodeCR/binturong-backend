using Domain.JournalEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Accounting;

internal sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("JournalEntryId");

        builder.Property(x => x.EntryType).HasMaxLength(20);
        builder.Property(x => x.Number).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(255);

        builder.Property(x => x.SourceModule).HasMaxLength(50);

        builder.Property(x => x.PeriodId).HasColumnName("PeriodId");

        builder.HasIndex(x => x.PeriodId);
        builder.HasIndex(x => x.EntryDate);

        builder
            .HasOne(x => x.Period)
            .WithMany(x => x.JournalEntries)
            .HasForeignKey(x => x.PeriodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(x => x.Details)
            .WithOne(x => x.JournalEntry)
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
