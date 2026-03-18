using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class PortalConfiguration : IEntityTypeConfiguration<Portal>
{
    public void Configure(EntityTypeBuilder<Portal> builder)
    {
        builder.ToTable("portals");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.CustomDomain)
            .HasMaxLength(255);

        builder.Property(e => e.Color)
            .HasMaxLength(50);

        builder.Property(e => e.HeaderText)
            .HasMaxLength(500);

        builder.Property(e => e.PageTitle)
            .HasMaxLength(255);

        builder.Property(e => e.HomepageLink)
            .HasMaxLength(2048);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => e.Slug)
            .IsUnique();

        builder.HasIndex(e => e.AccountId);

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Articles)
            .WithOne(a => a.Portal)
            .HasForeignKey(a => a.PortalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Categories)
            .WithOne(c => c.Portal)
            .HasForeignKey(c => c.PortalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
