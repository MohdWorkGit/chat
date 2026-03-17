using System.Net.Http.Json;
using System.Text.Json;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.EventHandlers;

public sealed class WebhookEventHandler :
    INotificationHandler<ConversationCreatedEvent>,
    INotificationHandler<MessageCreatedEvent>,
    INotificationHandler<ConversationStatusChangedEvent>,
    INotificationHandler<ContactCreatedEvent>
{
    private readonly IRepository<Webhook> _webhookRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookEventHandler> _logger;

    public WebhookEventHandler(
        IRepository<Webhook> webhookRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookEventHandler> logger)
    {
        _webhookRepository = webhookRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public Task Handle(ConversationCreatedEvent notification, CancellationToken cancellationToken)
    {
        var payload = new
        {
            Event = "conversation_created",
            notification.ConversationId,
            notification.AccountId
        };

        return DispatchWebhooksAsync(notification.AccountId, "conversation_created", payload, cancellationToken);
    }

    public Task Handle(MessageCreatedEvent notification, CancellationToken cancellationToken)
    {
        var payload = new
        {
            Event = "message_created",
            notification.MessageId,
            notification.ConversationId,
            notification.AccountId
        };

        return DispatchWebhooksAsync(notification.AccountId, "message_created", payload, cancellationToken);
    }

    public Task Handle(ConversationStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var payload = new
        {
            Event = "conversation_status_changed",
            notification.ConversationId,
            notification.AccountId,
            notification.PreviousStatus,
            notification.NewStatus
        };

        return DispatchWebhooksAsync(notification.AccountId, "conversation_status_changed", payload, cancellationToken);
    }

    public Task Handle(ContactCreatedEvent notification, CancellationToken cancellationToken)
    {
        var payload = new
        {
            Event = "contact_created",
            notification.ContactId,
            notification.AccountId
        };

        return DispatchWebhooksAsync(notification.AccountId, "contact_created", payload, cancellationToken);
    }

    private async Task DispatchWebhooksAsync(int accountId, string eventType, object payload, CancellationToken cancellationToken)
    {
        var webhooks = await _webhookRepository.FindAsync(
            w => w.AccountId == accountId,
            cancellationToken);

        if (webhooks.Count == 0)
            return;

        var httpClient = _httpClientFactory.CreateClient("Webhook");

        foreach (var webhook in webhooks)
        {
            if (!IsSubscribedToEvent(webhook, eventType))
                continue;

            _ = DeliverWebhookAsync(httpClient, webhook, eventType, payload, cancellationToken);
        }
    }

    private static bool IsSubscribedToEvent(Webhook webhook, string eventType)
    {
        // If no subscribed events are configured, the webhook receives all events
        if (string.IsNullOrWhiteSpace(webhook.SubscribedEvents))
            return true;

        var subscribedEvents = webhook.SubscribedEvents.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return subscribedEvents.Contains(eventType, StringComparer.OrdinalIgnoreCase);
    }

    private async Task DeliverWebhookAsync(HttpClient httpClient, Webhook webhook, string eventType, object payload, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Delivering webhook {WebhookId} for event {EventType} to {Url}",
                webhook.Id, eventType, webhook.Url);

            var response = await httpClient.PostAsJsonAsync(webhook.Url, payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Webhook {WebhookId} delivery to {Url} returned status {StatusCode}",
                    webhook.Id, webhook.Url, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delivering webhook {WebhookId} to {Url} for event {EventType}",
                webhook.Id, webhook.Url, eventType);
        }
    }
}
