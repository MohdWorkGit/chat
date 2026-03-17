namespace CustomerEngagement.Application.Services.Notifications;

public interface IPushNotificationService
{
    Task SendAsync(PushNotificationRequest request, CancellationToken cancellationToken = default);

    Task SendToUserAsync(int userId, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);

    Task RegisterDeviceTokenAsync(int userId, string deviceToken, string platform, CancellationToken cancellationToken = default);

    Task UnregisterDeviceTokenAsync(int userId, string deviceToken, CancellationToken cancellationToken = default);
}

public class PushNotificationRequest
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}
