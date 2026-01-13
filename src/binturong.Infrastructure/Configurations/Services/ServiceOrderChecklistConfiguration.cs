using Domain.ServiceOrderChecklists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.ServiceOrders;

internal sealed class ServiceOrderChecklistConfiguration
    : IEntityTypeConfiguration<ServiceOrderChecklist>
{
    public void Configure(EntityTypeBuilder<ServiceOrderChecklist> builder)
    {
        builder.ToTable("ServiceOrderChecklists");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ChecklistId");

        builder.Property(x => x.ServiceOrderId).HasColumnName("ServiceOrderId");
        builder.Property(x => x.Description).HasMaxLength(255).IsRequired();

        builder.HasIndex(x => x.ServiceOrderId);

        builder
            .HasOne(x => x.ServiceOrder)
            .WithMany(x => x.Checklists)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
