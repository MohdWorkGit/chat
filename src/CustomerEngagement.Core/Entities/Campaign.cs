using System.ComponentModel.DataAnnotations;
using System.Text.Json;
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
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

    [JsonPropertyName("audience")]
    public string? Audience { get; set; }

    public DateTime? ScheduledAt { get; set; }
    public bool Enabled { get; set; } = true;

    // Navigation properties
    public Account Account { get; set; } = null!;
    public Inbox Inbox { get; set; } = null!;

    /// <summary>
    /// True when the campaign has a scheduled date set.
    /// </summary>
    public bool IsScheduled => ScheduledAt.HasValue;

    /// <summary>
    /// True when the campaign type is Ongoing.
    /// </summary>
    public bool IsOngoing => CampaignType == CampaignType.Ongoing;

    /// <summary>
    /// Activates the campaign. Only Draft campaigns can be activated.
    /// </summary>
    public void Activate()
    {
        if (Status != CampaignStatus.Draft)
            throw new InvalidOperationException(
                $"Cannot activate a campaign with status '{Status}'. Only Draft campaigns can be activated.");

        Status = CampaignStatus.Active;
        Enabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the campaign by marking it as completed. Only Active campaigns can be deactivated.
    /// </summary>
    public void Deactivate()
    {
        if (Status != CampaignStatus.Active)
            throw new InvalidOperationException(
                $"Cannot deactivate a campaign with status '{Status}'. Only Active campaigns can be deactivated.");

        Status = CampaignStatus.Completed;
        Enabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deserializes the Audience JSON into a list of audience filter conditions.
    /// Returns an empty list if Audience is null or empty.
    /// </summary>
    public List<Dictionary<string, JsonElement>> GetAudienceFilter()
    {
        if (string.IsNullOrWhiteSpace(Audience))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(Audience) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
