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
