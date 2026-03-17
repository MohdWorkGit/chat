namespace CustomerEngagement.Core.Entities;

public class InboxMember : BaseEntity
{
    public int InboxId { get; set; }
    public int UserId { get; set; }

    // Navigation properties
    public Inbox Inbox { get; set; } = null!;
    public User User { get; set; } = null!;
}
