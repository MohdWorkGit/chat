using CustomerEngagement.Enterprise.Saml.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class SamlConfigConfiguration : IEntityTypeConfiguration<SamlConfig>
{
    public void Configure(EntityTypeBuilder<SamlConfig> builder)
    {
        builder.ToTable("saml_configs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.IdpEntityId)
            .IsRequired();

        builder.Property(e => e.IdpSsoTargetUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(e => e.IdpCertificate)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(e => e.SpEntityId)
            .IsRequired();

        builder.Property(e => e.AssertionConsumerServiceUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(e => e.NameIdentifierFormat)
            .HasMaxLength(255);

        builder.Property(e => e.Enabled)
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => e.AccountId)
            .IsUnique();

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
