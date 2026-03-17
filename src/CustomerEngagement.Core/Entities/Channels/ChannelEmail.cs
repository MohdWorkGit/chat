using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities.Channels;

public class ChannelEmail : BaseEntity
{
    public int InboxId { get; set; }

    [MaxLength(255)]
    public string? ForwardToEmail { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? ImapAddress { get; set; }

    public int? ImapPort { get; set; }

    [MaxLength(255)]
    public string? ImapLogin { get; set; }

    [MaxLength(255)]
    public string? ImapPassword { get; set; }

    public bool ImapEnabled { get; set; }

    [MaxLength(255)]
    public string? SmtpAddress { get; set; }

    public int? SmtpPort { get; set; }

    [MaxLength(255)]
    public string? SmtpLogin { get; set; }

    [MaxLength(255)]
    public string? SmtpPassword { get; set; }

    public bool SmtpEnabled { get; set; }

    // Navigation properties
    public Inbox Inbox { get; set; } = null!;
}
