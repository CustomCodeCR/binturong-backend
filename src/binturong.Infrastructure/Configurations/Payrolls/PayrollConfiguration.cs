using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Payrolls;

internal sealed class PayrollConfiguration : IEntityTypeConfiguration<Domain.Payrolls.Payroll>
{
    public void Configure(EntityTypeBuilder<Domain.Payrolls.Payroll> builder)
    {
        builder.ToTable("Payrolls");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("PayrollId");

        builder.Property(x => x.PeriodCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.PayrollType).HasMaxLength(20);
        builder.Property(x => x.Status).HasMaxLength(20);

        builder.HasIndex(x => x.PeriodCode);

        builder
            .HasMany(x => x.Details)
            .WithOne(x => x.Payroll)
            .HasForeignKey(x => x.PayrollId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
