using CustomerEngagement.Core.Entities;

namespace CustomerEngagement.Enterprise.Captain.Entities;

public class CaptainInbox : BaseEntity
{
    public int AssistantId { get; set; }

    public int InboxId { get; set; }

    public bool Active { get; set; } = true;

    // Navigation properties
    public CaptainAssistant Assistant { get; set; } = null!;
    public Inbox Inbox { get; set; } = null!;
}
