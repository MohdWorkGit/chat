using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class ReportingEvent : BaseEntity
{
    public int AccountId { get; set; }

    [MaxLength(255)]
    public string? Name { get; set; }

    public double? Value { get; set; }
    public DateTime? EventStartedAt { get; set; }
    public DateTime? EventEndedAt { get; set; }
    public int? ConversationId { get; set; }
    public int? UserId { get; set; }
    public int? InboxId { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
}
