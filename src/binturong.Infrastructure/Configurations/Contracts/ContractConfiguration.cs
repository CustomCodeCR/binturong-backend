using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Contracts;

internal sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ContractId");

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();

        builder.Property(x => x.ClientId).HasColumnName("ClientId");
        builder.Property(x => x.QuoteId).HasColumnName("QuoteId");
        builder.Property(x => x.SalesOrderId).HasColumnName("SalesOrderId");

        builder.Property(x => x.Status).HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.Notes).HasMaxLength(255);

        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.ClientId);

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.Contracts)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Quote)
            .WithMany(x => x.Contracts)
            .HasForeignKey(x => x.QuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.SalesOrder)
            .WithMany(x => x.Contracts)
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(x => x.BillingMilestones)
            .WithOne(x => x.Contract)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.ServiceOrders)
            .WithOne(x => x.Contract)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(x => x.Invoices)
            .WithOne(x => x.Contract)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
