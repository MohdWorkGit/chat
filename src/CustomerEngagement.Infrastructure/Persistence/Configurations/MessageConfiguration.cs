using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .HasColumnType("text");

        builder.Property(m => m.ContentType)
            .HasMaxLength(50)
            .HasDefaultValue("text");

        builder.Property(m => m.MessageType)
            .IsRequired();

        builder.Property(m => m.Status)
            .HasDefaultValue(MessageStatus.Sent);

        builder.Property(m => m.Private)
            .HasDefaultValue(false);

        builder.Property(m => m.SourceId)
            .HasMaxLength(255);

        builder.Property(m => m.ContentAttributes)
            .HasColumnType("jsonb");

        builder.Property(m => m.ExternalSourceIds)
            .HasColumnType("jsonb");

        builder.Property(m => m.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(m => m.UpdatedAt)
            .HasDefaultValueSql("now()");

        // Indexes
        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.AccountId);
        builder.HasIndex(m => m.SourceId);
        builder.HasIndex(m => new { m.ConversationId, m.CreatedAt });

        // Relationships
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Contact)
            .WithMany()
            .HasForeignKey(m => m.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(m => m.Attachments)
            .WithOne(a => a.Message)
            .HasForeignKey(a => a.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
