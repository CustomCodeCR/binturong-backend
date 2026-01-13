using Domain.ContractBillingMilestones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Contracts;

internal sealed class ContractBillingMilestoneConfiguration
    : IEntityTypeConfiguration<ContractBillingMilestone>
{
    public void Configure(EntityTypeBuilder<ContractBillingMilestone> builder)
    {
        builder.ToTable("ContractBillingMilestones");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("MilestoneId");

        builder.Property(x => x.ContractId).HasColumnName("ContractId");
        builder.Property(x => x.Description).HasMaxLength(255).IsRequired();

        builder.Property(x => x.Percentage).HasPrecision(10, 4);
        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.Property(x => x.InvoiceId).HasColumnName("InvoiceId");

        builder.HasIndex(x => x.ContractId);

        builder
            .HasOne(x => x.Contract)
            .WithMany(x => x.BillingMilestones)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Invoice)
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
