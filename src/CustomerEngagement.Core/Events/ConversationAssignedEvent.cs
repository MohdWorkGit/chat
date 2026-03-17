using MediatR;

namespace CustomerEngagement.Core.Events;

public sealed record ConversationAssignedEvent(
    int ConversationId,
    int AccountId,
    int? AssigneeId,
    int? TeamId) : INotification;
