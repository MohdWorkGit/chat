using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Locale)
            .HasMaxLength(10);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => new { e.PortalId, e.Slug, e.Locale })
            .IsUnique();

        builder.HasIndex(e => e.AccountId);

        builder.HasOne(e => e.ParentCategory)
            .WithMany()
            .HasForeignKey(e => e.ParentCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
