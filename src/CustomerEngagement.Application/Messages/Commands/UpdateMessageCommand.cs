using MediatR;

namespace CustomerEngagement.Application.Messages.Commands;

public record UpdateMessageCommand(long AccountId = 0, long ConversationId = 0, long Id = 0, string? Content = null) : IRequest<object>;

public record DeleteMessageCommand(long AccountId, long ConversationId, long Id) : IRequest<object>;
