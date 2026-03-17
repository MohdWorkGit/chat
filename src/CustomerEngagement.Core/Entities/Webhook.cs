using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerEngagement.Core.Entities;

public class Webhook : BaseEntity
{
    public int AccountId { get; set; }
    public int? InboxId { get; set; }

    [Required]
    [MaxLength(2048)]
    [Url]
    public required string Url { get; set; }

    [JsonPropertyName("subscribed_events")]
    public string? SubscribedEvents { get; set; }

    [MaxLength(255)]
    public string? HmacToken { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
}
