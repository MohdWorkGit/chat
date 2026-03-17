using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contacts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(255);

        builder.Property(c => c.Email)
            .HasMaxLength(255);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Identifier)
            .HasMaxLength(255);

        builder.Property(c => c.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(c => c.AdditionalAttributes)
            .HasColumnType("jsonb");

        builder.Property(c => c.CustomAttributes)
            .HasColumnType("jsonb");

        builder.Property(c => c.LastActivityAt)
            .IsRequired(false);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("now()");

        // Composite indexes
        builder.HasIndex(c => new { c.AccountId, c.Email })
            .HasFilter("\"Email\" IS NOT NULL");

        builder.HasIndex(c => new { c.AccountId, c.Phone })
            .HasFilter("\"Phone\" IS NOT NULL");

        builder.HasIndex(c => new { c.AccountId, c.Identifier })
            .IsUnique()
            .HasFilter("\"Identifier\" IS NOT NULL");

        // Full-text search index on Name and Email using PostgreSQL GIN
        builder.HasIndex(c => new { c.Name, c.Email })
            .HasMethod("gin")
            .HasAnnotation("Npgsql:TsVectorConfig", "english");

        builder.HasIndex(c => c.AccountId);

        // Relationships
        builder.HasOne(c => c.Account)
            .WithMany(a => a.Contacts)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Conversations)
            .WithOne(cv => cv.Contact)
            .HasForeignKey(cv => cv.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.ContactInboxes)
            .WithOne(ci => ci.Contact)
            .HasForeignKey(ci => ci.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Notes)
            .WithOne(n => n.Contact)
            .HasForeignKey(n => n.ContactId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
