using Domain.MarketingAssetTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Marketing;

internal sealed class MarketingAssetTrackingConfiguration
    : IEntityTypeConfiguration<MarketingAssetTrackingEntry>
{
    public void Configure(EntityTypeBuilder<MarketingAssetTrackingEntry> builder)
    {
        builder.ToTable("MarketingAssetTracking");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("TrackingId");

        builder.Property(x => x.AssetId).HasColumnName("AssetId");
        builder.Property(x => x.WebClientId).HasColumnName("WebClientId");
        builder.Property(x => x.ClientId).HasColumnName("ClientId");
        builder.Property(x => x.UserId).HasColumnName("UserId");

        builder.Property(x => x.EventType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.SessionId).HasMaxLength(100);
        builder.Property(x => x.MetadataJson).HasColumnType("text");

        builder.HasIndex(x => x.AssetId);
        builder.HasIndex(x => x.EventDate);

        builder
            .HasOne(x => x.Asset)
            .WithMany(x => x.Trackings)
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.WebClient)
            .WithMany(x => x.MarketingTrackings)
            .HasForeignKey(x => x.WebClientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.MarketingTrackings)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.User)
            .WithMany(x => x.MarketingTrackings)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
