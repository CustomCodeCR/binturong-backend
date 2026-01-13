using Domain.DebitNotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Invoices;

internal sealed class DebitNoteConfiguration : IEntityTypeConfiguration<DebitNote>
{
    public void Configure(EntityTypeBuilder<DebitNote> builder)
    {
        builder.ToTable("DebitNotes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("DebitNoteId");

        builder.Property(x => x.InvoiceId).HasColumnName("InvoiceId");
        builder.Property(x => x.TaxKey).HasMaxLength(50);
        builder.Property(x => x.Consecutive).HasMaxLength(50);
        builder.Property(x => x.Reason).HasMaxLength(255);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.TaxStatus).HasMaxLength(20);

        builder.HasIndex(x => x.InvoiceId);

        builder
            .HasOne(x => x.Invoice)
            .WithMany(x => x.DebitNotes)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
