using CustomerEngagement.Application.Services.Conversations;
using MediatR;

namespace CustomerEngagement.Application.Commands;

public record AssignConversationCommand(
    long ConversationId,
    int? AgentId,
    int? TeamId) : IRequest;

public class AssignConversationCommandHandler : IRequestHandler<AssignConversationCommand>
{
    private readonly IConversationService _conversationService;

    public AssignConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
    }

    public async Task Handle(AssignConversationCommand request, CancellationToken cancellationToken)
    {
        await _conversationService.AssignAsync(request.ConversationId, request.AgentId, request.TeamId, cancellationToken);
    }
}
