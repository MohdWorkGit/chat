using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Conversations;

public interface IMessageService
{
    Task<PaginatedResultDto<MessageDto>> GetByConversationAsync(
        long conversationId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<MessageDto> CreateAsync(
        long conversationId,
        CreateMessageRequest request,
        CancellationToken cancellationToken = default);

    Task<MessageDto> UpdateAsync(
        long messageId,
        UpdateMessageRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long messageId, CancellationToken cancellationToken = default);
}

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

public class AttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
}
