using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class AgentBotInboxConfiguration : IEntityTypeConfiguration<AgentBotInbox>
{
    public void Configure(EntityTypeBuilder<AgentBotInbox> builder)
    {
        builder.ToTable("agent_bot_inboxes");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .HasDefaultValue(0);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => new { e.AgentBotId, e.InboxId })
            .IsUnique();

        builder.HasIndex(e => e.AccountId);

        builder.HasOne(e => e.AgentBot)
            .WithMany()
            .HasForeignKey(e => e.AgentBotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Inbox)
            .WithMany()
            .HasForeignKey(e => e.InboxId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
