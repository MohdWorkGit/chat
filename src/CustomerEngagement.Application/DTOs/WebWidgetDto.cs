namespace CustomerEngagement.Application.DTOs;

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
    public bool PreChatFormEnabled { get; set; }
    public string? PreChatFormOptions { get; set; }
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
