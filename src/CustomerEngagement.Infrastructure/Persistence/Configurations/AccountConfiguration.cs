using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Domain)
            .HasMaxLength(255);

        builder.Property(a => a.Locale)
            .HasMaxLength(20)
            .HasDefaultValue("en");

        builder.Property(a => a.SupportEmail)
            .HasMaxLength(255);

        builder.Property(a => a.SettingsFlags)
            .HasColumnType("jsonb");

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(a => a.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(a => a.Name);
        builder.HasIndex(a => a.Domain).IsUnique().HasFilter("\"Domain\" IS NOT NULL");

        builder.HasMany(a => a.Conversations)
            .WithOne(c => c.Account)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Contacts)
            .WithOne(c => c.Account)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Inboxes)
            .WithOne(i => i.Account)
            .HasForeignKey(i => i.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Teams)
            .WithOne(t => t.Account)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.AccountUsers)
            .WithOne(au => au.Account)
            .HasForeignKey(au => au.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
