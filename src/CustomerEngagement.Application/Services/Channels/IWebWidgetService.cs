namespace CustomerEngagement.Application.Services.Channels;

public interface IWebWidgetService
{
    Task<WebWidgetConfigDto> CreateWidgetAsync(int accountId, CreateWebWidgetRequest request, CancellationToken cancellationToken = default);

    Task<WebWidgetConfigDto?> GetConfigAsync(string widgetToken, CancellationToken cancellationToken = default);

    Task<bool> ValidateTokenAsync(string widgetToken, CancellationToken cancellationToken = default);
}

public class WebWidgetConfigDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string WelcomeTitle { get; set; } = string.Empty;
    public string WelcomeTagline { get; set; } = string.Empty;
    public string WidgetColor { get; set; } = "#1F93FF";
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class CreateWebWidgetRequest
{
    public int InboxId { get; set; }
    public string WebsiteUrl { get; set; } = string.Empty;
    public string? WelcomeTitle { get; set; }
    public string? WelcomeTagline { get; set; }
    public string? WidgetColor { get; set; }
}
