using Domain.MarketingAudienceRules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Marketing;

internal sealed class MarketingAudienceRuleConfiguration
    : IEntityTypeConfiguration<MarketingAudienceRule>
{
    public void Configure(EntityTypeBuilder<MarketingAudienceRule> builder)
    {
        builder.ToTable("MarketingAudienceRules");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("RuleId");

        builder.Property(x => x.CampaignId).HasColumnName("CampaignId");

        builder.Property(x => x.RuleType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RuleValue).HasMaxLength(150);

        builder.HasIndex(x => x.CampaignId);

        builder
            .HasOne(x => x.Campaign)
            .WithMany(x => x.AudienceRules)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
