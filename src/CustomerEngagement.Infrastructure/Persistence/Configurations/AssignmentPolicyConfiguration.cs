using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class AssignmentPolicyConfiguration : IEntityTypeConfiguration<AssignmentPolicy>
{
    public void Configure(EntityTypeBuilder<AssignmentPolicy> builder)
    {
        builder.ToTable("assignment_policies");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PolicyType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.MaxAutoAssignLimit)
            .HasDefaultValue(0);

        builder.Property(e => e.Active)
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => new { e.AccountId, e.InboxId })
            .IsUnique();

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Inbox)
            .WithMany()
            .HasForeignKey(e => e.InboxId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
