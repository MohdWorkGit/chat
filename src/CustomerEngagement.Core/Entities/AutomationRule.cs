using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerEngagement.Core.Entities;

public class AutomationRule : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(255)]
    public required string EventName { get; set; }

    [JsonPropertyName("conditions")]
    public string? Conditions { get; set; }

    [JsonPropertyName("actions")]
    public string? Actions { get; set; }

    public bool Active { get; set; } = true;

    // Navigation properties
    public Account Account { get; set; } = null!;
}
