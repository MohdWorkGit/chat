using System.Text.Json.Serialization;

namespace CustomerEngagement.Core.Entities;

public class NotificationSetting : BaseEntity
{
    public int AccountId { get; set; }
    public int UserId { get; set; }

    [JsonPropertyName("email_flags")]
    public string? EmailFlags { get; set; }

    [JsonPropertyName("push_flags")]
    public string? PushFlags { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
