using CustomerEngagement.Enterprise.Captain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class CaptainInboxConfiguration : IEntityTypeConfiguration<CaptainInbox>
{
    public void Configure(EntityTypeBuilder<CaptainInbox> builder)
    {
        builder.ToTable("captain_inboxes");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Active)
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => new { e.AssistantId, e.InboxId })
            .IsUnique();

        builder.HasIndex(e => e.InboxId);

        builder.HasOne(e => e.Assistant)
            .WithMany(a => a.Inboxes)
            .HasForeignKey(e => e.AssistantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Inbox)
            .WithMany()
            .HasForeignKey(e => e.InboxId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
