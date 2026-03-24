using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using MediatR;

namespace CustomerEngagement.Application.Messages.Commands;

public record CreateMessageCommand(
    long AccountId,
    long ConversationId,
    string Content,
    string? ContentType,
    int MessageType,
    int? SenderId,
    string? SenderType,
    bool IsPrivate) : IRequest<MessageDto>;

public class CreateMessageCommandHandler : IRequestHandler<CreateMessageCommand, MessageDto>
{
    private readonly IMessageService _messageService;

    public CreateMessageCommandHandler(IMessageService messageService)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
    }

    public async Task<MessageDto> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new CreateMessageRequest
        {
            Content = request.Content,
            ContentType = request.ContentType ?? "text",
            MessageType = request.MessageType,
            SenderId = request.SenderId,
            SenderType = request.SenderType,
            IsPrivate = request.IsPrivate
        };

        return await _messageService.CreateAsync(request.ConversationId, createRequest, cancellationToken);
    }
}
