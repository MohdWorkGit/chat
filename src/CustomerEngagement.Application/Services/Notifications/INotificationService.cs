using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Notifications;

public interface INotificationService
{
    Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);

    Task<PaginatedResultDto<NotificationDto>> GetByUserAsync(
        int userId,
        int accountId,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    Task MarkReadAsync(int notificationId, CancellationToken cancellationToken = default);

    Task MarkAllReadAsync(int userId, int accountId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int notificationId, CancellationToken cancellationToken = default);
}
