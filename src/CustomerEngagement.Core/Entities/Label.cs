using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class Label : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    public bool ShowOnSidebar { get; set; } = true;

    public int? ConversationId { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
    public Conversation? Conversation { get; set; }
}
