using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class AssignmentPolicy : BaseEntity
{
    public int AccountId { get; set; }
    public int InboxId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string PolicyType { get; set; } // "round_robin", "manual", "auto"

    public int MaxAutoAssignLimit { get; set; }
    public bool Active { get; set; } = true;

    // Navigation properties
    public Account Account { get; set; } = null!;
    public Inbox Inbox { get; set; } = null!;
}
