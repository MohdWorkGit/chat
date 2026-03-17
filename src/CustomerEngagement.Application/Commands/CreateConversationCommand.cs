using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using MediatR;

namespace CustomerEngagement.Application.Commands;

public record CreateConversationCommand(
    int AccountId,
    int InboxId,
    int ContactId,
    string? Message,
    int? AssigneeId,
    int? TeamId,
    string? Status) : IRequest<ConversationDto>;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    private readonly IConversationService _conversationService;

    public CreateConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
    }

    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new Services.Conversations.CreateConversationRequest
        {
            InboxId = request.InboxId,
            ContactId = request.ContactId,
            InitialMessage = request.Message,
            AssigneeId = request.AssigneeId,
            TeamId = request.TeamId
        };

        return await _conversationService.CreateAsync(request.AccountId, createRequest, cancellationToken);
    }
}
