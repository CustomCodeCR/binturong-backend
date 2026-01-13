using Domain.PayrollDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Payroll;

internal sealed class PayrollDetailConfiguration : IEntityTypeConfiguration<PayrollDetail>
{
    public void Configure(EntityTypeBuilder<PayrollDetail> builder)
    {
        builder.ToTable("PayrollDetails");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("PayrollDetailId");

        builder.Property(x => x.PayrollId).HasColumnName("PayrollId");
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeId");

        builder.Property(x => x.GrossSalary).HasPrecision(18, 2);
        builder.Property(x => x.OvertimeHours).HasPrecision(18, 2);
        builder.Property(x => x.Deductions).HasPrecision(18, 2);
        builder.Property(x => x.EmployerContrib).HasPrecision(18, 2);
        builder.Property(x => x.NetSalary).HasPrecision(18, 2);

        builder.HasIndex(x => x.PayrollId);

        builder
            .HasOne(x => x.Payroll)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.PayrollId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Employee)
            .WithMany(x => x.PayrollDetails)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
