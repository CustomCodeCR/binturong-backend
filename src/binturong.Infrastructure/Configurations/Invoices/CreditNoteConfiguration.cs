using Domain.CreditNotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Invoices;

internal sealed class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
    public void Configure(EntityTypeBuilder<CreditNote> builder)
    {
        builder.ToTable("CreditNotes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("CreditNoteId");

        builder.Property(x => x.InvoiceId).HasColumnName("InvoiceId");
        builder.Property(x => x.TaxKey).HasMaxLength(50);
        builder.Property(x => x.Consecutive).HasMaxLength(50);
        builder.Property(x => x.Reason).HasMaxLength(255);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.TaxStatus).HasMaxLength(20);

        builder.HasIndex(x => x.InvoiceId);

        builder
            .HasOne(x => x.Invoice)
            .WithMany(x => x.CreditNotes)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
