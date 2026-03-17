using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class AgentBot : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(2048)]
    public string? OutgoingUrl { get; set; }

    [MaxLength(50)]
    public string? BotType { get; set; }

    public int? AccountId { get; set; }

    // Navigation properties
    public Account? Account { get; set; }
}
