using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Application.Services.Conversations;

public interface IConversationService
{
    Task<ConversationDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<PaginatedResultDto<ConversationDto>> GetByAccountAsync(
        int accountId,
        ConversationFilterDto filter,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    Task<ConversationDto> CreateAsync(
        int accountId,
        CreateConversationRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(long conversationId, ConversationStatus status, CancellationToken cancellationToken = default);

    Task AssignAsync(long conversationId, int? agentId, int? teamId, CancellationToken cancellationToken = default);

    Task TogglePriorityAsync(long conversationId, CancellationToken cancellationToken = default);

    Task MuteAsync(long conversationId, CancellationToken cancellationToken = default);

    Task UnmuteAsync(long conversationId, CancellationToken cancellationToken = default);

    Task AddParticipantAsync(long conversationId, long userId, int accountId, CancellationToken cancellationToken = default);

    Task RemoveParticipantAsync(long conversationId, long userId, int accountId, CancellationToken cancellationToken = default);

    Task SnoozeAsync(long conversationId, DateTime snoozeUntil, CancellationToken cancellationToken = default);

    Task ResolveAsync(long conversationId, CancellationToken cancellationToken = default);

    Task ReopenAsync(long conversationId, CancellationToken cancellationToken = default);
}

