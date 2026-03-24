using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
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

    Task SnoozeAsync(long conversationId, DateTime snoozeUntil, CancellationToken cancellationToken = default);

    Task ResolveAsync(long conversationId, CancellationToken cancellationToken = default);

    Task ReopenAsync(long conversationId, CancellationToken cancellationToken = default);
}

public class ConversationFilterDto
{
    public ConversationStatus? Status { get; set; }
    public int? AssigneeId { get; set; }
    public int? InboxId { get; set; }
    public int? TeamId { get; set; }
    public string? LabelName { get; set; }
    public string? Query { get; set; }
    public string SortBy { get; set; } = "created_at";
    public bool SortDescending { get; set; } = true;
}

public class CreateConversationRequest
{
    public int InboxId { get; set; }
    public int ContactId { get; set; }
    public string? InitialMessage { get; set; }
    public int? AssigneeId { get; set; }
    public int? TeamId { get; set; }
    public ConversationStatus Status { get; set; } = ConversationStatus.Open;
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
}

