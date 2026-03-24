using MediatR;

namespace CustomerEngagement.Application.Public.Commands;

public record SubmitCsatSurveyCommand(string SurveyToken = "", int? Rating = null, string? Feedback = null) : IRequest<object>;

public record CreatePublicContactCommand(string InboxIdentifier = "", string? Name = null, string? Email = null) : IRequest<object>;

public record CreatePublicConversationCommand(string InboxIdentifier = "", string? ContactIdentifier = null, string? Content = null) : IRequest<object>;

public record CreatePublicMessageCommand(string InboxIdentifier = "", long ConversationId = 0, string? Content = null) : IRequest<object>;
