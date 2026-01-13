using Domain.JournalEntryDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Accounting;

internal sealed class JournalEntryDetailConfiguration : IEntityTypeConfiguration<JournalEntryDetail>
{
    public void Configure(EntityTypeBuilder<JournalEntryDetail> builder)
    {
        builder.ToTable("JournalEntryDetails");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("JournalEntryDetailId");

        builder.Property(x => x.JournalEntryId).HasColumnName("JournalEntryId");
        builder.Property(x => x.AccountId).HasColumnName("AccountId");
        builder.Property(x => x.CostCenterId).HasColumnName("CostCenterId");

        builder.Property(x => x.Debit).HasPrecision(18, 2);
        builder.Property(x => x.Credit).HasPrecision(18, 2);

        builder.Property(x => x.Description).HasMaxLength(255);

        builder.HasIndex(x => x.JournalEntryId);

        builder
            .HasOne(x => x.JournalEntry)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Account)
            .WithMany(x => x.JournalEntryDetails)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.CostCenter)
            .WithMany(x => x.JournalEntryDetails)
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
