namespace CustomerEngagement.Application.DTOs;

public record MessageDto(
    int Id, int ConversationId, int AccountId, int? SenderId,
    string? SenderType, string? Content, string? ContentType,
    string MessageType, bool Private, string Status,
    DateTime? SentAt, DateTime CreatedAt,
    IReadOnlyList<AttachmentDto> Attachments);

