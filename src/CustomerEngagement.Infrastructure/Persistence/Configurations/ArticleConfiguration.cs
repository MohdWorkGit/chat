using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("articles");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(500);

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

        builder.HasIndex(e => new { e.PortalId, e.Slug })
            .IsUnique();

        builder.HasIndex(e => e.AccountId);
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.Status);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Articles)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Author)
            .WithMany()
            .HasForeignKey(e => e.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
