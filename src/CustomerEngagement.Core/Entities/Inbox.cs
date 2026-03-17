using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class Inbox : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(100)]
    public string? ChannelType { get; set; }

    public int? ChannelId { get; set; }

    public bool GreetingEnabled { get; set; }
    public string? GreetingMessage { get; set; }
    public bool EnableAutoAssignment { get; set; } = true;
    public string? OutOfOfficeMessage { get; set; }
    public bool WorkingHoursEnabled { get; set; }
    public bool CsatSurveyEnabled { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
    public ICollection<InboxMember> InboxMembers { get; set; } = [];
}
