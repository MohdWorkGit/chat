using CustomerEngagement.Core.Entities;

namespace CustomerEngagement.Enterprise.Captain.Entities;

public class CopilotThread : BaseEntity
{
    public int AccountId { get; set; }

    public int UserId { get; set; }

    public int AssistantId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public CaptainAssistant Assistant { get; set; } = null!;
    public ICollection<CopilotMessage> Messages { get; set; } = [];
}
