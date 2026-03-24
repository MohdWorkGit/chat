namespace CustomerEngagement.Application.DTOs;

public class InboundEmailRequest
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? MessageId { get; set; }
    public string? InReplyTo { get; set; }
    public List<EmailAttachmentInfo>? Attachments { get; set; }
}

public class OutboundEmailRequest
{
    public long ConversationId { get; set; }
    public long MessageId { get; set; }
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? ReplyToMessageId { get; set; }
}

public class EmailAttachmentInfo
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
}
