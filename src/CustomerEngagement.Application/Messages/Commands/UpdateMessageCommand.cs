using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using MediatR;

namespace CustomerEngagement.Application.Messages.Commands;

public record UpdateMessageCommand(long AccountId = 0, long ConversationId = 0, long Id = 0, string? Content = null) : IRequest<MessageDto>;

public class UpdateMessageCommandHandler : IRequestHandler<UpdateMessageCommand, MessageDto>
{
    private readonly IMessageService _messageService;

    public UpdateMessageCommandHandler(IMessageService messageService)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
    }

    public async Task<MessageDto> Handle(UpdateMessageCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateMessageRequest
        {
            Content = request.Content ?? string.Empty
        };

        return await _messageService.UpdateAsync(request.Id, updateRequest, cancellationToken);
    }
}

public record DeleteMessageCommand(long AccountId, long ConversationId, long Id) : IRequest;

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand>
{
    private readonly IMessageService _messageService;

    public DeleteMessageCommandHandler(IMessageService messageService)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
    }

    public async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        await _messageService.DeleteAsync(request.Id, cancellationToken);
    }
}
