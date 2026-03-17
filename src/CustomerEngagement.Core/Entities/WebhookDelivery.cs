namespace CustomerEngagement.Core.Entities;

public class WebhookDelivery : BaseEntity
{
    public int WebhookId { get; set; }
    public int AccountId { get; set; }
    public required string EventType { get; set; }
    public required string Payload { get; set; }
    public string? Signature { get; set; }
    public required string Status { get; set; }
    public int RetryCount { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // Navigation properties
    public Webhook Webhook { get; set; } = null!;
    public Account Account { get; set; } = null!;
}
