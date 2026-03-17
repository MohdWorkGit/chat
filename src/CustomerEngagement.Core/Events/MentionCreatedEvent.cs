using MediatR;

namespace CustomerEngagement.Core.Events;

public sealed record MentionCreatedEvent(
    int MentionId,
    int ConversationId,
    int AccountId,
    int MentionedUserId,
    int MessageId) : INotification;
