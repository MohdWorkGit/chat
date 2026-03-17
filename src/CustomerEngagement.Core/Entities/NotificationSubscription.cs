using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerEngagement.Core.Entities;

public class NotificationSubscription : BaseEntity
{
    public int UserId { get; set; }

    [MaxLength(100)]
    public string? SubscriptionType { get; set; }

    [JsonPropertyName("subscription_attributes")]
    public string? SubscriptionAttributes { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
