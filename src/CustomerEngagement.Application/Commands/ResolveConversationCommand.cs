using CustomerEngagement.Application.Services.Conversations;
using MediatR;

namespace CustomerEngagement.Application.Commands;

public record ResolveConversationCommand(long ConversationId) : IRequest;

public class ResolveConversationCommandHandler : IRequestHandler<ResolveConversationCommand>
{
    private readonly IConversationService _conversationService;

    public ResolveConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
    }

    public async Task Handle(ResolveConversationCommand request, CancellationToken cancellationToken)
    {
        await _conversationService.ResolveAsync(request.ConversationId, cancellationToken);
    }
}
