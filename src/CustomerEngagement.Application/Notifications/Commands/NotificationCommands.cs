using MediatR;

namespace CustomerEngagement.Application.Notifications.Commands;

public record MarkNotificationReadCommand(long AccountId, long NotificationId) : IRequest<object>;

public record MarkAllNotificationsReadCommand(long AccountId) : IRequest<object>;

public record DeleteNotificationCommand(long AccountId, long NotificationId) : IRequest<object>;
