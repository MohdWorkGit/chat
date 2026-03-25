using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

/// <summary>
/// Routes notifications to appropriate delivery channels (email, push).
/// Enqueued by Hangfire when a notification event is raised.
/// </summary>
public class NotificationDeliveryJob
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IRepository<NotificationSetting> _settingsRepository;
    private readonly IRepository<NotificationSubscription> _subscriptionRepository;
    // SubscriptionAttributes stores JSON with endpoint, keys, etc. for WebPush
    private readonly IRepository<User> _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly IWebPushSender _pushSender;
    private readonly ILogger<NotificationDeliveryJob> _logger;

    public NotificationDeliveryJob(
        IRepository<Notification> notificationRepository,
        IRepository<NotificationSetting> settingsRepository,
        IRepository<NotificationSubscription> subscriptionRepository,
        IRepository<User> userRepository,
        IEmailSender emailSender,
        IWebPushSender pushSender,
        ILogger<NotificationDeliveryJob> logger)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _pushSender = pushSender ?? throw new ArgumentNullException(nameof(pushSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification is null)
        {
            _logger.LogWarning("Notification {NotificationId} not found for delivery", notificationId);
            return;
        }

        var user = await _userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found for notification {NotificationId}", notification.UserId, notificationId);
            return;
        }

        // Check user notification preferences
        var settings = await _settingsRepository.FindAsync(
            s => s.UserId == notification.UserId && s.AccountId == notification.AccountId,
            cancellationToken);
        var userSettings = settings.FirstOrDefault();

        // Check email flags - if EmailFlags is null, email is enabled by default
        var emailEnabled = userSettings is null
            || string.IsNullOrEmpty(userSettings.EmailFlags)
            || userSettings.EmailFlags.Contains(notification.NotificationType ?? "", StringComparison.OrdinalIgnoreCase);

        if (emailEnabled && !string.IsNullOrEmpty(user.Email))
        {
            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email,
                    user.Name ?? user.Email,
                    $"New notification: {notification.NotificationType}",
                    $"<p>You have a new <strong>{notification.NotificationType}</strong> notification.</p>",
                    cancellationToken: cancellationToken);

                _logger.LogDebug("Email notification sent to {Email} for notification {NotificationId}",
                    user.Email, notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email notification {NotificationId} to {Email}",
                    notificationId, user.Email);
            }
        }

        // Send push notification to all active subscriptions
        var pushEnabled = userSettings is null
            || string.IsNullOrEmpty(userSettings.PushFlags)
            || userSettings.PushFlags.Contains(notification.NotificationType ?? "", StringComparison.OrdinalIgnoreCase);

        if (pushEnabled)
        {
            var subscriptions = await _subscriptionRepository.FindAsync(
                s => s.UserId == notification.UserId, cancellationToken);

            foreach (var subscription in subscriptions)
            {
                try
                {
                    await _pushSender.SendAsync(
                        subscription.SubscriptionAttributes ?? string.Empty,
                        notification.NotificationType ?? "notification",
                        $"New {notification.NotificationType} notification",
                        cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send push to subscription {SubscriptionId}", subscription.Id);
                }
            }
        }
    }
}
