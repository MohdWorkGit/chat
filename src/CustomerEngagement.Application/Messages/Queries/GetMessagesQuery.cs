using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using MediatR;

namespace CustomerEngagement.Application.Messages.Queries;

public record GetMessagesQuery(long AccountId, long ConversationId, int Page, int PageSize) : IRequest<MessageListDto>;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, MessageListDto>
{
    private readonly IMessageService _messageService;

    public GetMessagesQueryHandler(IMessageService messageService)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
    }

    public async Task<MessageListDto> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var result = await _messageService.GetByConversationAsync(
            request.ConversationId, request.Page, request.PageSize, cancellationToken);

        var totalPages = result.TotalCount > 0
            ? (int)Math.Ceiling((double)result.TotalCount / request.PageSize) : 0;
        var meta = new MetaDto(result.TotalCount, request.Page, request.PageSize, totalPages);

        return new MessageListDto(result.Items.ToList().AsReadOnly(), meta);
    }
}
