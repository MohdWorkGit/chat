using MediatR;

namespace CustomerEngagement.Core.Events;

public sealed record ConversationStatusChangedEvent(
    int ConversationId,
    int AccountId,
    string PreviousStatus,
    string NewStatus) : INotification;
