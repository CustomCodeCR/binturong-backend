using Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Invoices;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("InvoiceId");

        builder.Property(x => x.TaxKey).HasMaxLength(50);
        builder.Property(x => x.Consecutive).HasMaxLength(50);

        builder.Property(x => x.ClientId).HasColumnName("ClientId");
        builder.Property(x => x.BranchId).HasColumnName("BranchId");
        builder.Property(x => x.SalesOrderId).HasColumnName("SalesOrderId");
        builder.Property(x => x.ContractId).HasColumnName("ContractId");

        builder.Property(x => x.DocumentType).HasMaxLength(20);
        builder.Property(x => x.Currency).HasMaxLength(10);

        builder.Property(x => x.ExchangeRate).HasPrecision(18, 4);
        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.Taxes).HasPrecision(18, 2);
        builder.Property(x => x.Discounts).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.Property(x => x.TaxStatus).HasMaxLength(20);
        builder.Property(x => x.InternalStatus).HasMaxLength(20);

        builder.Property(x => x.PdfS3Key).HasMaxLength(500);
        builder.Property(x => x.XmlS3Key).HasMaxLength(500);

        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.TaxKey);
        builder.HasIndex(x => x.Consecutive);

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.Invoices)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Branch)
            .WithMany(x => x.Invoices)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.SalesOrder)
            .WithMany(x => x.Invoices)
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Contract)
            .WithMany(x => x.Invoices)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(x => x.Details)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.CreditNotes)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.DebitNotes)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.PaymentDetails)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.GatewayTransactions)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
