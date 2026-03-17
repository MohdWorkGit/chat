using MediatR;

namespace CustomerEngagement.Core.Events;

public sealed record ConversationUpdatedEvent(
    int ConversationId,
    int AccountId,
    IReadOnlyDictionary<string, object?> ChangedAttributes) : INotification;
