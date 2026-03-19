using CustomerEngagement.Enterprise.Saml.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class SamlRoleMappingConfiguration : IEntityTypeConfiguration<SamlRoleMapping>
{
    public void Configure(EntityTypeBuilder<SamlRoleMapping> builder)
    {
        builder.ToTable("saml_role_mappings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SamlAttributeValue)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.UserRole)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => new { e.SamlConfigId, e.SamlAttributeValue })
            .IsUnique();

        builder.HasOne(e => e.SamlConfig)
            .WithMany()
            .HasForeignKey(e => e.SamlConfigId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
