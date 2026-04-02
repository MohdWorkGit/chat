using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class ConversationDraftConfiguration : IEntityTypeConfiguration<ConversationDraft>
{
    public void Configure(EntityTypeBuilder<ConversationDraft> builder)
    {
        builder.ToTable("conversation_drafts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Content).HasColumnType("text");
        builder.Property(e => e.ContentType).HasMaxLength(50).HasDefaultValue("text");

        builder.HasOne(e => e.Conversation)
            .WithMany()
            .HasForeignKey(e => e.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: one draft per user per conversation
        builder.HasIndex(e => new { e.ConversationId, e.UserId, e.AccountId })
            .IsUnique();
    }
}
