using Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Employees;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("EmployeeId");

        builder.Property(x => x.UserId).HasColumnName("UserId");
        builder.Property(x => x.BranchId).HasColumnName("BranchId");

        builder.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.NationalId).HasMaxLength(30);
        builder.Property(x => x.JobTitle).HasMaxLength(100);

        builder.Property(x => x.BaseSalary).HasPrecision(18, 2);

        builder.HasIndex(x => x.UserId);

        builder
            .HasOne(x => x.User)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Branch)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(x => x.History)
            .WithOne(x => x.Employee)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
