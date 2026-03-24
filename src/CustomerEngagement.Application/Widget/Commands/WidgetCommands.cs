using MediatR;

namespace CustomerEngagement.Application.Widget.Commands;

public record CreateWidgetContactCommand(string WidgetToken = "", string? Name = null, string? Email = null) : IRequest<object>;

public record UpdateWidgetContactCommand(string WidgetToken = "", string ContactIdentifier = "", string? Name = null, string? Email = null) : IRequest<object>;

public record CreateWidgetConversationCommand(string WidgetToken = "", string? ContactIdentifier = null, string? Content = null) : IRequest<object>;

public record SendWidgetMessageCommand(string WidgetToken = "", long ConversationId = 0, string? Content = null) : IRequest<object>;
