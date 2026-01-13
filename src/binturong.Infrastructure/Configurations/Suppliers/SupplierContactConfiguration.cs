using Domain.SupplierContacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Suppliers;

internal sealed class SupplierContactConfiguration : IEntityTypeConfiguration<SupplierContact>
{
    public void Configure(EntityTypeBuilder<SupplierContact> builder)
    {
        builder.ToTable("SupplierContacts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ContactId");

        builder.Property(x => x.SupplierId).HasColumnName("SupplierId");

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.JobTitle).HasMaxLength(100);
        builder.Property(x => x.Email).HasMaxLength(150);
        builder.Property(x => x.Phone).HasMaxLength(50);

        builder.HasIndex(x => x.SupplierId);

        builder
            .HasOne(x => x.Supplier)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
