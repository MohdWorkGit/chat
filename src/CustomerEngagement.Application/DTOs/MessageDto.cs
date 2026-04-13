namespace CustomerEngagement.Application.DTOs;

public record MessageDto(
    int Id, int ConversationId, int AccountId, int? SenderId,
    string? SenderType, string? Content, string? ContentType,
    string MessageType, bool Private, string Status,
    DateTime? SentAt, DateTime CreatedAt,
    IReadOnlyList<AttachmentDto> Attachments);

public class CreateMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public int MessageType { get; set; }
    public int? SenderId { get; set; }
    public string? SenderType { get; set; }
    public bool IsPrivate { get; set; }
    public string? ContentType { get; set; } = "text";
    public List<AttachmentRequest>? Attachments { get; set; }
}

public class UpdateMessageRequest
{
    public string Content { get; set; } = string.Empty;
}

public record MessageListDto(IReadOnlyList<MessageDto> Data, MetaDto Meta);

public class AttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
}

