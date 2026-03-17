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

public class CreateNotificationRequest
{
    public int AccountId { get; set; }
    public int UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string? PrimaryActorType { get; set; }
    public int? PrimaryActorId { get; set; }
    public string? SecondaryActorType { get; set; }
    public int? SecondaryActorId { get; set; }
    public long? ConversationId { get; set; }
    public long? MessageId { get; set; }
}
