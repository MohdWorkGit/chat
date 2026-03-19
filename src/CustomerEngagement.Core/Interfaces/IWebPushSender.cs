namespace CustomerEngagement.Core.Interfaces;

public interface IWebPushSender
{
    Task SendAsync(string subscriptionToken, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);
}
