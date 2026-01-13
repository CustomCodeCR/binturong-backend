using Domain.ServiceOrderMaterials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.ServiceOrders;

internal sealed class ServiceOrderMaterialConfiguration
    : IEntityTypeConfiguration<ServiceOrderMaterial>
{
    public void Configure(EntityTypeBuilder<ServiceOrderMaterial> builder)
    {
        builder.ToTable("ServiceOrderMaterials");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ServiceOrderMaterialId");

        builder.Property(x => x.ServiceOrderId).HasColumnName("ServiceOrderId");
        builder.Property(x => x.ProductId).HasColumnName("ProductId");

        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.EstimatedCost).HasPrecision(18, 4);

        builder.HasIndex(x => x.ServiceOrderId);

        builder
            .HasOne(x => x.ServiceOrder)
            .WithMany(x => x.Materials)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Product)
            .WithMany(x => x.ServiceOrderMaterials)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
