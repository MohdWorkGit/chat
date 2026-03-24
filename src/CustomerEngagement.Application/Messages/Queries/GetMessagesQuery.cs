using MediatR;

namespace CustomerEngagement.Application.Messages.Queries;

public record GetMessagesQuery(long AccountId, long ConversationId, int Page, int PageSize) : IRequest<object>;
