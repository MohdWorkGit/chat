using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class ContactInbox : BaseEntity
{
    public int ContactId { get; set; }
    public int InboxId { get; set; }

    [MaxLength(255)]
    public string? SourceId { get; set; }

    // Navigation properties
    public Contact Contact { get; set; } = null!;
    public Inbox Inbox { get; set; } = null!;
}
