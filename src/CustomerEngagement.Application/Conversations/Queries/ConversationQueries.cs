using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using MediatR;

namespace CustomerEngagement.Application.Conversations.Queries;

public record GetConversationsQuery(
    long AccountId,
    string? Status,
    long? InboxId,
    long? AssigneeId,
    long? TeamId,
    string? Label,
    int Page = 1,
    int PageSize = 25) : IRequest<ConversationListDto>;

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, ConversationListDto>
{
    private readonly IConversationService _conversationService;

    public GetConversationsQueryHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task<ConversationListDto> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var filter = new ConversationFilterDto
        {
            InboxId = request.InboxId.HasValue ? (int)request.InboxId.Value : null,
            AssigneeId = request.AssigneeId.HasValue ? (int)request.AssigneeId.Value : null,
            TeamId = request.TeamId.HasValue ? (int)request.TeamId.Value : null,
            LabelName = request.Label
        };

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ConversationStatus>(request.Status, true, out var status))
            filter.Status = status;

        var result = await _conversationService.GetByAccountAsync(
            (int)request.AccountId, filter, request.Page, request.PageSize, cancellationToken);

        var totalPages = result.TotalCount > 0
            ? (int)Math.Ceiling((double)result.TotalCount / request.PageSize) : 0;
        var meta = new MetaDto(result.TotalCount, request.Page, request.PageSize, totalPages);

        return new ConversationListDto(result.Items.ToList().AsReadOnly(), meta);
    }
}

public record GetConversationByIdQuery(long AccountId, long ConversationId) : IRequest<ConversationDto?>;

public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, ConversationDto?>
{
    private readonly IConversationService _conversationService;

    public GetConversationByIdQueryHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task<ConversationDto?> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
    {
        return await _conversationService.GetByIdAsync(request.ConversationId, cancellationToken);
    }
}

public record GetParticipantsQuery(long AccountId, long ConversationId) : IRequest<IReadOnlyList<UserSummaryDto>>;

public class GetParticipantsQueryHandler : IRequestHandler<GetParticipantsQuery, IReadOnlyList<UserSummaryDto>>
{
    public Task<IReadOnlyList<UserSummaryDto>> Handle(GetParticipantsQuery request, CancellationToken cancellationToken)
    {
        // Participants would be loaded from the ConversationParticipant join table
        IReadOnlyList<UserSummaryDto> result = Array.Empty<UserSummaryDto>();
        return Task.FromResult(result);
    }
}
