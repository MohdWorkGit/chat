using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.DisplayId)
            .IsRequired();

        builder.Property(c => c.Status)
            .IsRequired()
            .HasDefaultValue(ConversationStatus.Open);

        builder.Property(c => c.Priority)
            .HasDefaultValue(ConversationPriority.None);

        builder.Property(c => c.Uuid)
            .HasMaxLength(255);

        builder.Property(c => c.AdditionalAttributes)
            .HasColumnType("jsonb");

        builder.Property(c => c.CustomAttributes)
            .HasColumnType("jsonb");

        builder.Property(c => c.SnoozedUntil)
            .IsRequired(false);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("now()");

        // Composite indexes
        builder.HasIndex(c => new { c.AccountId, c.DisplayId })
            .IsUnique();

        builder.HasIndex(c => new { c.AccountId, c.Status });

        builder.HasIndex(c => c.ContactId);
        builder.HasIndex(c => c.InboxId);
        builder.HasIndex(c => c.AssigneeId);
        builder.HasIndex(c => c.TeamId);

        // Relationships
        builder.HasOne(c => c.Contact)
            .WithMany(ct => ct.Conversations)
            .HasForeignKey(c => c.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Inbox)
            .WithMany(i => i.Conversations)
            .HasForeignKey(c => c.InboxId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Assignee)
            .WithMany(u => u.AssignedConversations)
            .HasForeignKey(c => c.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Team)
            .WithMany(t => t.Conversations)
            .HasForeignKey(c => c.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Account)
            .WithMany(a => a.Conversations)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Participants)
            .WithOne(p => p.Conversation)
            .HasForeignKey(p => p.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Labels)
            .WithOne(l => l.Conversation)
            .HasForeignKey(l => l.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
