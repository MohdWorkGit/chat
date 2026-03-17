using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using MediatR;

namespace CustomerEngagement.Application.Queries;

public record GetConversationsQuery(
    int AccountId,
    string? Status,
    int? InboxId,
    int? AssigneeId,
    int? TeamId,
    int Page = 1,
    int PageSize = 25) : IRequest<ConversationListDto>;

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, ConversationListDto>
{
    private readonly IConversationService _conversationService;

    public GetConversationsQueryHandler(IConversationService conversationService)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
    }

    public async Task<ConversationListDto> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var filter = new ConversationFilterDto
        {
            InboxId = request.InboxId,
            AssigneeId = request.AssigneeId,
            TeamId = request.TeamId
        };

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ConversationStatus>(request.Status, true, out var status))
        {
            filter.Status = status;
        }

        var result = await _conversationService.GetByAccountAsync(
            request.AccountId, filter, request.Page, request.PageSize, cancellationToken);

        var totalPages = result.TotalCount > 0
            ? (int)Math.Ceiling((double)result.TotalCount / request.PageSize)
            : 0;

        var meta = new MetaDto(result.TotalCount, request.Page, request.PageSize, totalPages);

        return new ConversationListDto(result.Items.ToList().AsReadOnly(), meta);
    }
}
