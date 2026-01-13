using Domain.SupplierAttachments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Suppliers;

internal sealed class SupplierAttachmentConfiguration : IEntityTypeConfiguration<SupplierAttachment>
{
    public void Configure(EntityTypeBuilder<SupplierAttachment> builder)
    {
        builder.ToTable("SupplierAttachments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("AttachmentId");

        builder.Property(x => x.SupplierId).HasColumnName("SupplierId");

        builder.Property(x => x.FileName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FileS3Key).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DocumentType).HasMaxLength(100);

        builder.Property(x => x.UploadedAt);

        builder.HasIndex(x => x.SupplierId);

        builder
            .HasOne(x => x.Supplier)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
