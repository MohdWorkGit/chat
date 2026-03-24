using MediatR;

namespace CustomerEngagement.Application.Widget.Queries;

public record GetWidgetConfigQuery(string WebsiteToken) : IRequest<object>;

public record GetWidgetConversationsQuery(string WidgetToken, string ContactIdentifier) : IRequest<object>;

public record GetWidgetMessagesQuery(string WidgetToken, long ConversationId, int Page, int PageSize) : IRequest<object>;
