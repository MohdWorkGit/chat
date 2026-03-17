using MediatR;

namespace CustomerEngagement.Core.Events;

public sealed record MessageCreatedEvent(int MessageId, int ConversationId, int AccountId) : INotification;
