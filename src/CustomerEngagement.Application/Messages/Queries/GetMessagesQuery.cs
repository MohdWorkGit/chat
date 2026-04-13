using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using MediatR;

namespace CustomerEngagement.Application.Messages.Queries;

public record GetMessagesQuery(long AccountId, long ConversationId, int Page, int PageSize) : IRequest<PaginatedResultDto<MessageDto>>;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, PaginatedResultDto<MessageDto>>
{
    private readonly IMessageService _messageService;

    public GetMessagesQueryHandler(IMessageService messageService)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
    }

    public async Task<PaginatedResultDto<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        return await _messageService.GetByConversationAsync(
            request.ConversationId, request.Page, request.PageSize, cancellationToken);
    }
}
