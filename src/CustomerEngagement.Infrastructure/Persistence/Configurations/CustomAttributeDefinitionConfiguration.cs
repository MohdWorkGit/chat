using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class CustomAttributeDefinitionConfiguration : IEntityTypeConfiguration<CustomAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<CustomAttributeDefinition> builder)
    {
        builder.ToTable("custom_attribute_definitions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.AttributeDisplayName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.AttributeDisplayType)
            .HasMaxLength(50);

        builder.Property(e => e.AttributeDescription)
            .HasMaxLength(1000);

        builder.Property(e => e.AttributeKey)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.AttributeModel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ListValues);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => new { e.AccountId, e.AttributeKey, e.AttributeModel })
            .IsUnique();

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
