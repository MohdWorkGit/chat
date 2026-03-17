using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class CsatSurveyResponse : BaseEntity
{
    public int AccountId { get; set; }
    public int ConversationId { get; set; }
    public int MessageId { get; set; }
    public int ContactId { get; set; }
    public int? AssigneeId { get; set; }
    public int? Rating { get; set; }

    [MaxLength(2000)]
    public string? FeedbackText { get; set; }

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
}
