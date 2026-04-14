using CustomerEngagement.Application.Notifications;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Notifications.Queries;

public record GetNotificationsQuery(long AccountId, int Page, int PageSize) : IRequest<object>;

public record GetUnreadNotificationCountQuery(long AccountId) : IRequest<object>;

public record GetNotificationSettingsQuery(long AccountId, long UserId) : IRequest<object>;

public class GetNotificationSettingsQueryHandler : IRequestHandler<GetNotificationSettingsQuery, object>
{
    private readonly IRepository<NotificationSetting> _settingsRepository;

    public GetNotificationSettingsQueryHandler(IRepository<NotificationSetting> settingsRepository)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
    }

    public async Task<object> Handle(GetNotificationSettingsQuery request, CancellationToken cancellationToken)
    {
        var existing = (await _settingsRepository.FindAsync(
                s => s.AccountId == (int)request.AccountId && s.UserId == (int)request.UserId,
                cancellationToken))
            .FirstOrDefault();

        var emailFlags = NotificationFlagMapping.ParseEmail(existing?.EmailFlags);
        var pushFlags = NotificationFlagMapping.ParsePush(existing?.PushFlags);

        return new
        {
            Id = existing?.Id ?? 0,
            AccountId = (int)request.AccountId,
            UserId = (int)request.UserId,
            EmailConversationCreation = emailFlags.ConversationCreation,
            EmailConversationAssignment = emailFlags.ConversationAssignment,
            EmailNewMessage = emailFlags.NewMessage,
            EmailMention = emailFlags.Mention,
            PushConversationCreation = pushFlags.ConversationCreation,
            PushConversationAssignment = pushFlags.ConversationAssignment,
            PushNewMessage = pushFlags.NewMessage,
            PushMention = pushFlags.Mention
        };
    }
}

public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, object>
{
    private readonly IRepository<Notification> _notificationRepository;

    public GetUnreadNotificationCountQueryHandler(IRepository<Notification> notificationRepository)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    }

    public async Task<object> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _notificationRepository.CountAsync(
            n => n.AccountId == (int)request.AccountId && n.ReadAt == null,
            cancellationToken);

        return new { Count = count };
    }
}

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, object>
{
    private readonly IRepository<Notification> _notificationRepository;

    public GetNotificationsQueryHandler(IRepository<Notification> notificationRepository)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    }

    public async Task<object> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            predicate: n => n.AccountId == (int)request.AccountId,
            orderBy: n => n.CreatedAt,
            ascending: false,
            cancellationToken: cancellationToken);

        var totalCount = await _notificationRepository.CountAsync(
            n => n.AccountId == (int)request.AccountId, cancellationToken);

        return new
        {
            Data = notifications.Select(n => new
            {
                n.Id,
                n.AccountId,
                n.UserId,
                n.NotificationType,
                n.PrimaryActorType,
                n.PrimaryActorId,
                n.SecondaryActorType,
                n.SecondaryActorId,
                n.ReadAt,
                n.CreatedAt
            }).ToList(),
            Meta = new
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        };
    }
}
