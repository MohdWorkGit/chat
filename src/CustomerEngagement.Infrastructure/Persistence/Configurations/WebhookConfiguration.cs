using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class WebhookConfiguration : IEntityTypeConfiguration<Webhook>
{
    public void Configure(EntityTypeBuilder<Webhook> builder)
    {
        builder.ToTable("webhooks");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Url)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(e => e.SubscribedEvents)
            .HasColumnType("jsonb");

        builder.Property(e => e.HmacToken)
            .HasMaxLength(255);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => e.AccountId);

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
