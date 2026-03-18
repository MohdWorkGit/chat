using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class RelatedCategoryConfiguration : IEntityTypeConfiguration<RelatedCategory>
{
    public void Configure(EntityTypeBuilder<RelatedCategory> builder)
    {
        builder.ToTable("related_categories");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => new { e.CategoryId, e.RelatedCategoryId })
            .IsUnique();

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Related)
            .WithMany()
            .HasForeignKey(e => e.RelatedCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
