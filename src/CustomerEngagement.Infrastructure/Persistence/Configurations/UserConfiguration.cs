using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(u => u.DisplayName)
            .HasMaxLength(255);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(u => u.Provider)
            .HasMaxLength(50)
            .HasDefaultValue("email");

        builder.Property(u => u.Uid)
            .HasMaxLength(255);

        builder.Property(u => u.CustomAttributes)
            .HasColumnType("jsonb");

        builder.Property(u => u.MessageSignature)
            .HasColumnType("text");

        builder.Property(u => u.Availability)
            .HasDefaultValue(0);

        builder.Property(u => u.Type)
            .HasMaxLength(50);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(u => u.UpdatedAt)
            .HasDefaultValueSql("now()");

        // Indexes
        builder.HasIndex(u => u.Uid).IsUnique().HasFilter("\"Uid\" IS NOT NULL");
        builder.HasIndex(u => u.Email).IsUnique();

        // Relationships
        builder.HasMany(u => u.AccountUsers)
            .WithOne(au => au.User)
            .HasForeignKey(au => au.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.AssignedConversations)
            .WithOne(c => c.Assignee)
            .HasForeignKey(c => c.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.InboxMembers)
            .WithOne(im => im.User)
            .HasForeignKey(im => im.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.TeamMembers)
            .WithOne(tm => tm.User)
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Mentions)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
