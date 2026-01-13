using Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.ServiceOrders;

internal sealed class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable("ServiceOrders");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ServiceOrderId");

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();

        builder.Property(x => x.ClientId).HasColumnName("ClientId");
        builder.Property(x => x.BranchId).HasColumnName("BranchId");
        builder.Property(x => x.ContractId).HasColumnName("ContractId");

        builder.Property(x => x.Status).HasMaxLength(20);
        builder.Property(x => x.ServiceAddress).HasMaxLength(255);
        builder.Property(x => x.Notes).HasMaxLength(255);

        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.ClientId);

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.ServiceOrders)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Branch)
            .WithMany(x => x.ServiceOrders)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Contract)
            .WithMany(x => x.ServiceOrders)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(x => x.Technicians)
            .WithOne(x => x.ServiceOrder)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Services)
            .WithOne(x => x.ServiceOrder)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Materials)
            .WithOne(x => x.ServiceOrder)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Checklists)
            .WithOne(x => x.ServiceOrder)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Photos)
            .WithOne(x => x.ServiceOrder)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
