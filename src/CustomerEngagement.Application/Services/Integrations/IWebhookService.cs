using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Integrations;

public interface IWebhookService
{
    Task<WebhookDto> RegisterAsync(int accountId, RegisterWebhookRequest request, CancellationToken cancellationToken = default);

    Task<IEnumerable<WebhookDto>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int webhookId, CancellationToken cancellationToken = default);

    Task FireWebhookAsync(int accountId, string eventType, object payload, CancellationToken cancellationToken = default);
}
