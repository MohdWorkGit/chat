namespace CustomerEngagement.Application.Services.Integrations;

public interface IWebhookService
{
    Task<WebhookDto> RegisterAsync(int accountId, RegisterWebhookRequest request, CancellationToken cancellationToken = default);

    Task<IEnumerable<WebhookDto>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int webhookId, CancellationToken cancellationToken = default);

    Task FireWebhookAsync(int accountId, string eventType, object payload, CancellationToken cancellationToken = default);
}

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
