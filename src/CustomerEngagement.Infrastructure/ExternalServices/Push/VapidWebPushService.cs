using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebPush;

namespace CustomerEngagement.Infrastructure.ExternalServices.Push;

public record PushSubscriptionInfo(string Endpoint, string P256dh, string Auth);

public record PushNotificationPayload(string Title, string Body, string? Icon = null, string? Url = null, object? Data = null);

public class VapidWebPushService
{
    private readonly WebPushClient _pushClient;
    private readonly VapidDetails _vapidDetails;
    private readonly ILogger<VapidWebPushService> _logger;

    public VapidWebPushService(IConfiguration configuration, ILogger<VapidWebPushService> logger)
    {
        _logger = logger;

        var subject = configuration["VAPID_SUBJECT"] ?? "mailto:admin@example.com";
        var publicKey = configuration["VAPID_PUBLIC_KEY"]
            ?? throw new InvalidOperationException("VAPID_PUBLIC_KEY is not configured.");
        var privateKey = configuration["VAPID_PRIVATE_KEY"]
            ?? throw new InvalidOperationException("VAPID_PRIVATE_KEY is not configured.");

        _vapidDetails = new VapidDetails(subject, publicKey, privateKey);
        _pushClient = new WebPushClient();
    }

    public async Task SendNotificationAsync(
        PushSubscriptionInfo subscription,
        PushNotificationPayload payload,
        CancellationToken cancellationToken = default)
    {
        var pushSubscription = new PushSubscription(
            subscription.Endpoint,
            subscription.P256dh,
            subscription.Auth);

        var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        try
        {
            await _pushClient.SendNotificationAsync(pushSubscription, jsonPayload, _vapidDetails, cancellationToken);
            _logger.LogDebug("Push notification sent to endpoint {Endpoint}", subscription.Endpoint);
        }
        catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone ||
                                          ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Push subscription expired or invalid for endpoint {Endpoint}. Status: {Status}",
                subscription.Endpoint, ex.StatusCode);
            throw new PushSubscriptionExpiredException(subscription.Endpoint, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to endpoint {Endpoint}", subscription.Endpoint);
            throw;
        }
    }

    public async Task SendNotificationToManyAsync(
        IEnumerable<PushSubscriptionInfo> subscriptions,
        PushNotificationPayload payload,
        CancellationToken cancellationToken = default)
    {
        var tasks = subscriptions.Select(sub =>
            SendNotificationSafeAsync(sub, payload, cancellationToken));

        await Task.WhenAll(tasks);
    }

    private async Task SendNotificationSafeAsync(
        PushSubscriptionInfo subscription,
        PushNotificationPayload payload,
        CancellationToken cancellationToken)
    {
        try
        {
            await SendNotificationAsync(subscription, payload, cancellationToken);
        }
        catch (PushSubscriptionExpiredException)
        {
            // Caller should handle cleanup of expired subscriptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to {Endpoint}", subscription.Endpoint);
        }
    }
}

public class PushSubscriptionExpiredException : Exception
{
    public string Endpoint { get; }

    public PushSubscriptionExpiredException(string endpoint, Exception innerException)
        : base($"Push subscription expired for endpoint: {endpoint}", innerException)
    {
        Endpoint = endpoint;
    }
}
