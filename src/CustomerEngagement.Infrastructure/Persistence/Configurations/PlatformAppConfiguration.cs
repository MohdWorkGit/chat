using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class PlatformAppConfiguration : IEntityTypeConfiguration<PlatformApp>
{
    public void Configure(EntityTypeBuilder<PlatformApp> builder)
    {
        builder.ToTable("platform_apps");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasMany(e => e.PlatformAppPermissibles)
            .WithOne()
            .HasForeignKey(p => p.PlatformAppId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
