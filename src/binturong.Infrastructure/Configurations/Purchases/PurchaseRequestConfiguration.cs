using Domain.PurchaseRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Purchases;

internal sealed class PurchaseRequestConfiguration : IEntityTypeConfiguration<PurchaseRequest>
{
    public void Configure(EntityTypeBuilder<PurchaseRequest> builder)
    {
        builder.ToTable("PurchaseRequests");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("RequestId");

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();

        builder.Property(x => x.BranchId).HasColumnName("BranchId");
        builder.Property(x => x.RequestedById).HasColumnName("RequestedById");

        builder.Property(x => x.Status).HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(255);

        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.BranchId);

        builder
            .HasOne(x => x.Branch)
            .WithMany(x => x.PurchaseRequests)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.RequestedBy)
            .WithMany(x => x.PurchaseRequests)
            .HasForeignKey(x => x.RequestedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(x => x.PurchaseOrders)
            .WithOne(x => x.Request)
            .HasForeignKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
