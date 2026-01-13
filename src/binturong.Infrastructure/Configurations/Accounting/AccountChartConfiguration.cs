using Domain.AccountsChart;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Accounting;

internal sealed class AccountChartConfiguration : IEntityTypeConfiguration<AccountChart>
{
    public void Configure(EntityTypeBuilder<AccountChart> builder)
    {
        builder.ToTable("AccountsChart");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("AccountId");

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();

        builder.Property(x => x.ParentAccountId).HasColumnName("ParentAccountId");
        builder.Property(x => x.CostCenterId).HasColumnName("CostCenterId");

        builder.Property(x => x.Status).HasMaxLength(20);

        builder.HasIndex(x => x.Code).IsUnique();

        builder
            .HasOne(x => x.ParentAccount)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.CostCenter)
            .WithMany(x => x.Accounts)
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
