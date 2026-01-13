using Domain.MarketingAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Marketing;

internal sealed class MarketingAssetConfiguration : IEntityTypeConfiguration<MarketingAsset>
{
    public void Configure(EntityTypeBuilder<MarketingAsset> builder)
    {
        builder.ToTable("MarketingAssets");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("AssetId");

        builder.Property(x => x.CampaignId).HasColumnName("CampaignId");

        builder.Property(x => x.AssetType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(150);
        builder.Property(x => x.Body).HasColumnType("text");

        builder.Property(x => x.ImageS3Key).HasMaxLength(500);
        builder.Property(x => x.LinkUrl).HasMaxLength(500);
        builder.Property(x => x.RouteStartsWith).HasMaxLength(200);

        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.CampaignId);

        builder
            .HasOne(x => x.Campaign)
            .WithMany(x => x.Assets)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Trackings)
            .WithOne(x => x.Asset)
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
