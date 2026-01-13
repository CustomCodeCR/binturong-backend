using Domain.ServiceOrderTechnicians;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.ServiceOrders;

internal sealed class ServiceOrderTechnicianConfiguration
    : IEntityTypeConfiguration<ServiceOrderTechnician>
{
    public void Configure(EntityTypeBuilder<ServiceOrderTechnician> builder)
    {
        builder.ToTable("ServiceOrderTechnicians");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ServiceOrderTechId");

        builder.Property(x => x.ServiceOrderId).HasColumnName("ServiceOrderId");
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeId");

        builder.Property(x => x.TechRole).HasMaxLength(50);

        builder.HasIndex(x => x.ServiceOrderId);

        builder
            .HasOne(x => x.ServiceOrder)
            .WithMany(x => x.Technicians)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Employee)
            .WithMany(x => x.ServiceOrderTechnicians)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
