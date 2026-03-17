using MediatR;

namespace CustomerEngagement.Core.Events;

public sealed record ConversationCreatedEvent(int ConversationId, int AccountId) : INotification;
