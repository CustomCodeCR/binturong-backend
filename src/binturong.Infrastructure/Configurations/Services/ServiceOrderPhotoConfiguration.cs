using Domain.ServiceOrderPhotos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.ServiceOrders;

internal sealed class ServiceOrderPhotoConfiguration : IEntityTypeConfiguration<ServiceOrderPhoto>
{
    public void Configure(EntityTypeBuilder<ServiceOrderPhoto> builder)
    {
        builder.ToTable("ServiceOrderPhotos");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("PhotoId");

        builder.Property(x => x.ServiceOrderId).HasColumnName("ServiceOrderId");
        builder.Property(x => x.PhotoS3Key).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);

        builder.HasIndex(x => x.ServiceOrderId);

        builder
            .HasOne(x => x.ServiceOrder)
            .WithMany(x => x.Photos)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
