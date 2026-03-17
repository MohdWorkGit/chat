namespace CustomerEngagement.Core.Entities;

public class ConversationParticipant : BaseEntity
{
    public int AccountId { get; set; }
    public int ConversationId { get; set; }
    public int UserId { get; set; }

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public User User { get; set; } = null!;
}
