namespace CustomerEngagement.Application.Services.Channels;

public interface IApiChannelService
{
    Task<ApiChannelMessageResult> ProcessInboundMessageAsync(ApiInboundMessageRequest request, CancellationToken cancellationToken = default);

    Task<ApiChannelMessageResult> SendOutboundMessageAsync(ApiOutboundMessageRequest request, CancellationToken cancellationToken = default);
}

public class ApiInboundMessageRequest
{
    public int AccountId { get; set; }
    public int InboxId { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ContentType { get; set; } = "text";
    public ContactIdentifier? Contact { get; set; }
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
}

public class ApiOutboundMessageRequest
{
    public long ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ContentType { get; set; } = "text";
    public int? SenderId { get; set; }
}

public class ContactIdentifier
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ExternalId { get; set; }
    public string? Name { get; set; }
}

public class ApiChannelMessageResult
{
    public bool Success { get; set; }
    public long? ConversationId { get; set; }
    public long? MessageId { get; set; }
    public string? Error { get; set; }
}
