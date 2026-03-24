namespace CustomerEngagement.Application.DTOs;

public class WebhookDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Url { get; set; } = string.Empty;
    public List<string> EventTypes { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public string? Secret { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RegisterWebhookRequest
{
    public string Url { get; set; } = string.Empty;
    public List<string> EventTypes { get; set; } = new();
    public string? Secret { get; set; }
}
