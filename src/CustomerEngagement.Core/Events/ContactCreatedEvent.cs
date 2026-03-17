using MediatR;

namespace CustomerEngagement.Core.Events;

public sealed record ContactCreatedEvent(int ContactId, int AccountId) : INotification;
