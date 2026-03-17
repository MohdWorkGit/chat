namespace CustomerEngagement.Core.Entities;

public class AgentBotInbox : BaseEntity
{
    public int AgentBotId { get; set; }
    public int InboxId { get; set; }
    public int AccountId { get; set; }
    public int Status { get; set; }

    // Navigation properties
    public AgentBot AgentBot { get; set; } = null!;
    public Inbox Inbox { get; set; } = null!;
}
