using MediatR;

namespace CustomerEngagement.Core.Events;

public sealed record ContactUpdatedEvent(int ContactId, int AccountId) : INotification;
