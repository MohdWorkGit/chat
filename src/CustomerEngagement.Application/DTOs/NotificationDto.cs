namespace CustomerEngagement.Application.DTOs;

public class CreateNotificationRequest
{
    public int AccountId { get; set; }
    public int UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string? PrimaryActorType { get; set; }
    public int? PrimaryActorId { get; set; }
    public string? SecondaryActorType { get; set; }
    public int? SecondaryActorId { get; set; }
    public long? ConversationId { get; set; }
    public long? MessageId { get; set; }
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

public class PushNotificationRequest
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}
