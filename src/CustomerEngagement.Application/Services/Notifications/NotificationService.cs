using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;

namespace CustomerEngagement.Application.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(
        IRepository<Notification> notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            AccountId = request.AccountId,
            UserId = request.UserId,
            NotificationType = request.NotificationType,
            PrimaryActorType = request.PrimaryActorType,
            PrimaryActorId = request.PrimaryActorId,
            SecondaryActorType = request.SecondaryActorType,
            SecondaryActorId = request.SecondaryActorId,
            ReadAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(notification);
    }

    public async Task<PaginatedResultDto<NotificationDto>> GetByUserAsync(
        int userId,
        int accountId,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.ListAsync(
            new { UserId = userId, AccountId = accountId },
            cancellationToken);

        var totalCount = notifications.Count;

        var items = notifications
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        return new PaginatedResultDto<NotificationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task MarkReadAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken)
            ?? throw new InvalidOperationException($"Notification {notificationId} not found.");

        notification.ReadAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;

        await _notificationRepository.UpdateAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(int userId, int accountId, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.ListAsync(
            new { UserId = userId, AccountId = accountId, Unread = true },
            cancellationToken);

        foreach (var notification in notifications)
        {
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken)
            ?? throw new InvalidOperationException($"Notification {notificationId} not found.");

        await _notificationRepository.DeleteAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.NotificationType,
            notification.PrimaryActorType,
            notification.PrimaryActorId,
            notification.SecondaryActorType,
            notification.SecondaryActorId,
            notification.IsRead,
            notification.CreatedAt);
    }
}
