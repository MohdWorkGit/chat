using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Integrations;

public class WebhookService : IWebhookService
{
    private readonly IRepository<Webhook> _webhookRepository;
    private readonly IRepository<WebhookDelivery> _deliveryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        IRepository<Webhook> webhookRepository,
        IRepository<WebhookDelivery> deliveryRepository,
        IUnitOfWork unitOfWork,
        ILogger<WebhookService> logger)
    {
        _webhookRepository = webhookRepository ?? throw new ArgumentNullException(nameof(webhookRepository));
        _deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WebhookDto> RegisterAsync(int accountId, RegisterWebhookRequest request, CancellationToken cancellationToken = default)
    {
        var webhook = new Webhook
        {
            AccountId = accountId,
            Url = request.Url,
            EventTypes = JsonSerializer.Serialize(request.EventTypes),
            Secret = request.Secret,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _webhookRepository.AddAsync(webhook, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(webhook);
    }

    public async Task<IEnumerable<WebhookDto>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var webhooks = await _webhookRepository.ListAsync(new { AccountId = accountId }, cancellationToken);
        return webhooks.Select(MapToDto);
    }

    public async Task DeleteAsync(int webhookId, CancellationToken cancellationToken = default)
    {
        var webhook = await _webhookRepository.GetByIdAsync(webhookId, cancellationToken)
            ?? throw new InvalidOperationException($"Webhook {webhookId} not found.");

        await _webhookRepository.DeleteAsync(webhook, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task FireWebhookAsync(int accountId, string eventType, object payload, CancellationToken cancellationToken = default)
    {
        var webhooks = await _webhookRepository.ListAsync(
            new { AccountId = accountId, IsActive = true },
            cancellationToken);

        var payloadJson = JsonSerializer.Serialize(payload);

        foreach (var webhook in webhooks)
        {
            var eventTypes = JsonSerializer.Deserialize<List<string>>(webhook.EventTypes) ?? new();
            if (!eventTypes.Contains(eventType) && !eventTypes.Contains("*"))
                continue;

            try
            {
                // Queue the delivery for async processing
                var delivery = new WebhookDelivery
                {
                    WebhookId = webhook.Id,
                    AccountId = accountId,
                    EventType = eventType,
                    Payload = payloadJson,
                    Signature = ComputeSignature(payloadJson, webhook.Secret),
                    Status = "queued",
                    RetryCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _deliveryRepository.AddAsync(delivery, cancellationToken);
                _logger.LogInformation(
                    "Webhook delivery queued for webhook {WebhookId}, event {EventType}",
                    webhook.Id, eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to queue webhook delivery for webhook {WebhookId}, event {EventType}",
                    webhook.Id, eventType);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? ComputeSignature(string payload, string? secret)
    {
        if (string.IsNullOrEmpty(secret))
            return null;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static WebhookDto MapToDto(Webhook webhook)
    {
        return new WebhookDto
        {
            Id = webhook.Id,
            AccountId = webhook.AccountId,
            Url = webhook.Url,
            EventTypes = JsonSerializer.Deserialize<List<string>>(webhook.EventTypes) ?? new(),
            IsActive = webhook.IsActive,
            CreatedAt = webhook.CreatedAt,
            UpdatedAt = webhook.UpdatedAt
        };
    }
}
