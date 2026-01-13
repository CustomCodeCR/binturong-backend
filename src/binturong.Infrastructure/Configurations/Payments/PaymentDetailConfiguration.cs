using Domain.PaymentDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Payments;

internal sealed class PaymentDetailConfiguration : IEntityTypeConfiguration<PaymentDetail>
{
    public void Configure(EntityTypeBuilder<PaymentDetail> builder)
    {
        builder.ToTable("PaymentDetails");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("PaymentDetailId");

        builder.Property(x => x.PaymentId).HasColumnName("PaymentId");
        builder.Property(x => x.InvoiceId).HasColumnName("InvoiceId");

        builder.Property(x => x.AppliedAmount).HasPrecision(18, 2);

        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.InvoiceId);

        builder
            .HasOne(x => x.Payment)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Invoice)
            .WithMany(x => x.PaymentDetails)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
