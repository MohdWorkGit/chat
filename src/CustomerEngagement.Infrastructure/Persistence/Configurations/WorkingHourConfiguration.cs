using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class WorkingHourConfiguration : IEntityTypeConfiguration<WorkingHour>
{
    public void Configure(EntityTypeBuilder<WorkingHour> builder)
    {
        builder.ToTable("working_hours");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => new { e.InboxId, e.DayOfWeek })
            .IsUnique();

        builder.HasIndex(e => e.AccountId);

        builder.HasOne(e => e.Inbox)
            .WithMany()
            .HasForeignKey(e => e.InboxId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
