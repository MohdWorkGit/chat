using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class ConversationDraft : BaseEntity
{
    public int ConversationId { get; set; }
    public int AccountId { get; set; }
    public int UserId { get; set; }

    public string? Content { get; set; }

    [MaxLength(50)]
    public string? ContentType { get; set; } = "text";

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public User User { get; set; } = null!;
}
