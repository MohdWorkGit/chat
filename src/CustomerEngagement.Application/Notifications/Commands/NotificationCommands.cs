using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Notifications.Commands;

public record MarkNotificationReadCommand(long AccountId, long NotificationId) : IRequest<object>;

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, object>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadCommandHandler(
        IRepository<Notification> notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync((int)request.NotificationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Notification with ID {request.NotificationId} not found.");

        if (notification.AccountId != (int)request.AccountId)
        {
            throw new KeyNotFoundException($"Notification with ID {request.NotificationId} not found.");
        }

        if (!notification.IsRead)
        {
            notification.MarkRead();
            _notificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new
        {
            notification.Id,
            notification.AccountId,
            notification.UserId,
            notification.NotificationType,
            notification.ReadAt,
            notification.UpdatedAt
        };
    }
}

public record MarkAllNotificationsReadCommand(long AccountId, long UserId) : IRequest<object>;

public class MarkAllNotificationsReadCommandHandler : IRequestHandler<MarkAllNotificationsReadCommand, object>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllNotificationsReadCommandHandler(
        IRepository<Notification> notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var accountId = (int)request.AccountId;
        var userId = (int)request.UserId;

        var unread = await _notificationRepository.FindAsync(
            n => n.AccountId == accountId && n.UserId == userId && n.ReadAt == null,
            cancellationToken);

        var count = 0;
        foreach (var notification in unread)
        {
            notification.MarkRead();
            _notificationRepository.Update(notification);
            count++;
        }

        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new { Count = count };
    }
}

public record DeleteNotificationCommand(long AccountId, long NotificationId) : IRequest<object>;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, object>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNotificationCommandHandler(
        IRepository<Notification> notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync((int)request.NotificationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Notification with ID {request.NotificationId} not found.");

        if (notification.AccountId != (int)request.AccountId)
        {
            throw new KeyNotFoundException($"Notification with ID {request.NotificationId} not found.");
        }

        _notificationRepository.Remove(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new { Deleted = true };
    }
}

public record SnoozeNotificationCommand(long AccountId, long NotificationId, DateTime SnoozedUntil) : IRequest<object>;

public class SnoozeNotificationCommandHandler : IRequestHandler<SnoozeNotificationCommand, object>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SnoozeNotificationCommandHandler(
        IRepository<Notification> notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(SnoozeNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync((int)request.NotificationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Notification with ID {request.NotificationId} not found.");

        if (notification.AccountId != (int)request.AccountId)
        {
            throw new KeyNotFoundException($"Notification with ID {request.NotificationId} not found.");
        }

        notification.Snooze(request.SnoozedUntil);
        _notificationRepository.Update(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new
        {
            notification.Id,
            notification.AccountId,
            notification.UserId,
            notification.NotificationType,
            notification.PrimaryActorType,
            notification.PrimaryActorId,
            notification.SecondaryActorType,
            notification.SecondaryActorId,
            notification.ReadAt,
            notification.SnoozedUntil,
            notification.CreatedAt,
            notification.UpdatedAt
        };
    }
}

public record UpdateNotificationSettingsCommand(
    long AccountId,
    long UserId,
    bool? EmailConversationCreation,
    bool? EmailConversationAssignment,
    bool? EmailNewMessage,
    bool? EmailMention,
    bool? PushConversationCreation,
    bool? PushConversationAssignment,
    bool? PushNewMessage,
    bool? PushMention) : IRequest<object>;

public class UpdateNotificationSettingsCommandHandler : IRequestHandler<UpdateNotificationSettingsCommand, object>
{
    private readonly IRepository<NotificationSetting> _settingsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNotificationSettingsCommandHandler(
        IRepository<NotificationSetting> settingsRepository,
        IUnitOfWork unitOfWork)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
    {
        var existing = (await _settingsRepository.FindAsync(
                s => s.AccountId == (int)request.AccountId && s.UserId == (int)request.UserId,
                cancellationToken))
            .FirstOrDefault();

        var isNew = existing is null;
        var setting = existing ?? new NotificationSetting
        {
            AccountId = (int)request.AccountId,
            UserId = (int)request.UserId,
        };

        // Start from current state (default = all enabled when flags are null/empty).
        var emailFlags = NotificationFlagMapping.ParseEmail(setting.EmailFlags);
        var pushFlags = NotificationFlagMapping.ParsePush(setting.PushFlags);

        if (request.EmailConversationCreation.HasValue) emailFlags.ConversationCreation = request.EmailConversationCreation.Value;
        if (request.EmailConversationAssignment.HasValue) emailFlags.ConversationAssignment = request.EmailConversationAssignment.Value;
        if (request.EmailNewMessage.HasValue) emailFlags.NewMessage = request.EmailNewMessage.Value;
        if (request.EmailMention.HasValue) emailFlags.Mention = request.EmailMention.Value;

        if (request.PushConversationCreation.HasValue) pushFlags.ConversationCreation = request.PushConversationCreation.Value;
        if (request.PushConversationAssignment.HasValue) pushFlags.ConversationAssignment = request.PushConversationAssignment.Value;
        if (request.PushNewMessage.HasValue) pushFlags.NewMessage = request.PushNewMessage.Value;
        if (request.PushMention.HasValue) pushFlags.Mention = request.PushMention.Value;

        setting.EmailFlags = NotificationFlagMapping.Serialize(emailFlags);
        setting.PushFlags = NotificationFlagMapping.Serialize(pushFlags);
        setting.UpdatedAt = DateTime.UtcNow;

        if (isNew)
        {
            await _settingsRepository.AddAsync(setting, cancellationToken);
        }
        else
        {
            _settingsRepository.Update(setting);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new
        {
            setting.Id,
            setting.AccountId,
            setting.UserId,
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
