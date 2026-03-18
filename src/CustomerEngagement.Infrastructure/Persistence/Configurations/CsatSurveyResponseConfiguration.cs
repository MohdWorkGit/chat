using CustomerEngagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class CsatSurveyResponseConfiguration : IEntityTypeConfiguration<CsatSurveyResponse>
{
    public void Configure(EntityTypeBuilder<CsatSurveyResponse> builder)
    {
        builder.ToTable("csat_survey_responses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FeedbackText)
            .HasMaxLength(2000);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => e.AccountId);
        builder.HasIndex(e => e.ConversationId);
        builder.HasIndex(e => e.ContactId);
        builder.HasIndex(e => e.AssigneeId);

        builder.HasOne(e => e.Conversation)
            .WithMany()
            .HasForeignKey(e => e.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Contact)
            .WithMany()
            .HasForeignKey(e => e.ContactId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
