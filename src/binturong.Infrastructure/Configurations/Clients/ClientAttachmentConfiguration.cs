using Domain.ClientAttachments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Clients;

internal sealed class ClientAttachmentConfiguration : IEntityTypeConfiguration<ClientAttachment>
{
    public void Configure(EntityTypeBuilder<ClientAttachment> builder)
    {
        builder.ToTable("ClientAttachments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("AttachmentId");

        builder.Property(x => x.ClientId).HasColumnName("ClientId");

        builder.Property(x => x.FileName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FileS3Key).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DocumentType).HasMaxLength(100);

        builder.Property(x => x.UploadedAt);

        builder.HasIndex(x => x.ClientId);

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
