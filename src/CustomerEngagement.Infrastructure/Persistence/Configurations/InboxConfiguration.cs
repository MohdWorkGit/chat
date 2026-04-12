using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class InboxConfiguration : IEntityTypeConfiguration<Inbox>
{
    public void Configure(EntityTypeBuilder<Inbox> builder)
    {
        builder.ToTable("inboxes");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(i => i.ChannelType)
            .HasMaxLength(100);

        builder.Property(i => i.ChannelId);

        builder.Property(i => i.GreetingEnabled)
            .HasDefaultValue(false);

        builder.Property(i => i.GreetingMessage)
            .HasColumnType("text");

        builder.Property(i => i.EnableAutoAssignment)
            .HasDefaultValue(true);

        builder.Property(i => i.EnableEmailCollect)
            .HasDefaultValue(true);

        builder.Property(i => i.CsatSurveyEnabled)
            .HasDefaultValue(false);

        builder.Property(i => i.AllowMessagesAfterResolved)
            .HasDefaultValue(true);

        builder.Property(i => i.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(i => i.UpdatedAt)
            .HasDefaultValueSql("now()");

        // Indexes
        builder.HasIndex(i => i.AccountId);
        builder.HasIndex(i => new { i.ChannelType, i.ChannelId });

        // Relationships
        builder.HasOne(i => i.Account)
            .WithMany(a => a.Inboxes)
            .HasForeignKey(i => i.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Conversations)
            .WithOne(c => c.Inbox)
            .HasForeignKey(c => c.InboxId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.InboxMembers)
            .WithOne(im => im.Inbox)
            .HasForeignKey(im => im.InboxId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.ContactInboxes)
            .WithOne(ci => ci.Inbox)
            .HasForeignKey(ci => ci.InboxId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
