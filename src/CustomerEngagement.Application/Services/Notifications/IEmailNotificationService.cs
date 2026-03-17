namespace CustomerEngagement.Application.Services.Notifications;

public interface IEmailNotificationService
{
    Task SendAsync(EmailNotificationRequest request, CancellationToken cancellationToken = default);

    Task SendConversationAssignmentNotificationAsync(int userId, long conversationId, CancellationToken cancellationToken = default);

    Task SendNewMessageNotificationAsync(int userId, long conversationId, long messageId, CancellationToken cancellationToken = default);

    Task SendMentionNotificationAsync(int userId, long conversationId, long messageId, int mentionedByUserId, CancellationToken cancellationToken = default);
}

public class EmailNotificationRequest
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? TemplateName { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
}
