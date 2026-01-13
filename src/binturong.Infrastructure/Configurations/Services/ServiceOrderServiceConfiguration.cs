using Domain.ServiceOrderServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.ServiceOrders;

internal sealed class ServiceOrderServiceConfiguration
    : IEntityTypeConfiguration<ServiceOrderService>
{
    public void Configure(EntityTypeBuilder<ServiceOrderService> builder)
    {
        builder.ToTable("ServiceOrderServices");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ServiceOrderServiceId");

        builder.Property(x => x.ServiceOrderId).HasColumnName("ServiceOrderId");
        builder.Property(x => x.ServiceId).HasColumnName("ServiceId");

        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.RateApplied).HasPrecision(18, 2);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);

        builder.HasIndex(x => x.ServiceOrderId);

        builder
            .HasOne(x => x.ServiceOrder)
            .WithMany(x => x.Services)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Service)
            .WithMany(x => x.ServiceOrderServices)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
