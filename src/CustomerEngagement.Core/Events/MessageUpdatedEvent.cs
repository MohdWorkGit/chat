using MediatR;

namespace CustomerEngagement.Core.Events;

public sealed record MessageUpdatedEvent(int MessageId, int ConversationId, int AccountId) : INotification;
