using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Core.Entities;

public class Campaign : BaseEntity
{
    public int AccountId { get; set; }
    public int InboxId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string? Message { get; set; }

    public CampaignType CampaignType { get; set; } = CampaignType.Ongoing;

    [JsonPropertyName("audience")]
    public string? Audience { get; set; }

    public DateTime? ScheduledAt { get; set; }
    public bool Enabled { get; set; } = true;

    // Navigation properties
    public Account Account { get; set; } = null!;
    public Inbox Inbox { get; set; } = null!;
}
