using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerEngagement.Core.Entities.Channels;

public class ChannelWebWidget : BaseEntity
{
    public int InboxId { get; set; }

    [MaxLength(2048)]
    public string? WebsiteUrl { get; set; }

    [MaxLength(255)]
    public string? WebsiteToken { get; set; }

    [MaxLength(500)]
    public string? WelcomeTitle { get; set; }

    [MaxLength(500)]
    public string? WelcomeTagline { get; set; }

    [MaxLength(50)]
    public string? WidgetColor { get; set; }

    [MaxLength(50)]
    public string? ReplyTime { get; set; }

    [MaxLength(255)]
    public string? HmacToken { get; set; }

    public bool PreChatFormEnabled { get; set; }

    [JsonPropertyName("pre_chat_form_options")]
    public string? PreChatFormOptions { get; set; }

    // Navigation properties
    public Inbox Inbox { get; set; } = null!;
}
