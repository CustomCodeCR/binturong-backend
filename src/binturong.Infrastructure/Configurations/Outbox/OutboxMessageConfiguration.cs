using Domain.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Outbox;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("OutboxId");

        builder.Property(x => x.Type).HasMaxLength(200).IsRequired();

        builder.Property(x => x.PayloadJson).HasColumnType("text").IsRequired();

        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();

        builder.Property(x => x.Attempts).IsRequired();

        builder.Property(x => x.LastError).HasColumnType("text").IsRequired(false);

        builder.Property(x => x.NextAttemptAt).IsRequired(false);

        builder.Property(x => x.LockedUntil).IsRequired(false);

        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.NextAttemptAt);
        builder.HasIndex(x => x.LockedUntil);
    }
}
