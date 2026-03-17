using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

public class WebhookDeliveryJob
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

    private readonly IRepository<Webhook> _webhookRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WebhookDeliveryJob> _logger;

    public WebhookDeliveryJob(
        IRepository<Webhook> webhookRepository,
        IHttpClientFactory httpClientFactory,
        IUnitOfWork unitOfWork,
        ILogger<WebhookDeliveryJob> logger)
    {
        _webhookRepository = webhookRepository ?? throw new ArgumentNullException(nameof(webhookRepository));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Delivers a webhook event to the registered URL with HMAC signature and retry logic.
    /// Intended to be enqueued by Hangfire as a fire-and-forget job.
    /// </summary>
    public async Task ExecuteAsync(int webhookId, string eventName, string payloadJson, CancellationToken cancellationToken = default)
    {
        var webhook = await _webhookRepository.GetByIdAsync(webhookId, cancellationToken);
        if (webhook is null)
        {
            _logger.LogWarning("Webhook {WebhookId} not found, skipping delivery", webhookId);
            return;
        }

        var lastException = (Exception?)null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                _logger.LogInformation(
                    "Retrying webhook delivery for {WebhookId}, attempt {Attempt}/{MaxRetries} after {Delay}s",
                    webhookId, attempt, MaxRetries, delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }

            try
            {
                await SendWebhookAsync(webhook, eventName, payloadJson, cancellationToken);
                _logger.LogInformation(
                    "Webhook delivery succeeded for {WebhookId}, event {EventName}",
                    webhookId, eventName);
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex,
                    "Webhook delivery failed for {WebhookId}, event {EventName}, attempt {Attempt}/{MaxRetries}",
                    webhookId, eventName, attempt + 1, MaxRetries + 1);
            }
        }

        _logger.LogError(lastException,
            "Webhook delivery permanently failed for {WebhookId}, event {EventName} after {MaxRetries} retries",
            webhookId, eventName, MaxRetries);
    }

    private async Task SendWebhookAsync(Webhook webhook, string eventName, string payloadJson, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient("WebhookDelivery");
        client.Timeout = RequestTimeout;

        using var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url);
        request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
        request.Headers.Add("X-Webhook-Event", eventName);

        if (!string.IsNullOrEmpty(webhook.HmacToken))
        {
            var signature = ComputeHmacSignature(payloadJson, webhook.HmacToken);
            request.Headers.Add("X-Webhook-Signature", signature);
        }

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static string ComputeHmacSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
