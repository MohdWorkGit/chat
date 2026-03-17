using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using MediatR;

namespace CustomerEngagement.Application.Queries;

public record GetConversationByIdQuery(long ConversationId) : IRequest<ConversationDto?>;

public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, ConversationDto?>
{
    private readonly IConversationService _conversationService;

    public GetConversationByIdQueryHandler(IConversationService conversationService)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
    }

    public async Task<ConversationDto?> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
    {
        return await _conversationService.GetByIdAsync(request.ConversationId, cancellationToken);
    }
}
