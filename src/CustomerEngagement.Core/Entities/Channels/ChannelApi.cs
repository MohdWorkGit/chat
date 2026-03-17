using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities.Channels;

public class ChannelApi : BaseEntity
{
    public int InboxId { get; set; }

    [MaxLength(2048)]
    public string? WebhookUrl { get; set; }

    [MaxLength(255)]
    public string? HmacToken { get; set; }

    [MaxLength(255)]
    public string? Identifier { get; set; }

    // Navigation properties
    public Inbox Inbox { get; set; } = null!;
}
