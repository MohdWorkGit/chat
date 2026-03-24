using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Notifications;

public interface IEmailNotificationService
{
    Task SendAsync(EmailNotificationRequest request, CancellationToken cancellationToken = default);

    Task SendConversationAssignmentNotificationAsync(int userId, long conversationId, CancellationToken cancellationToken = default);

    Task SendNewMessageNotificationAsync(int userId, long conversationId, long messageId, CancellationToken cancellationToken = default);

    Task SendMentionNotificationAsync(int userId, long conversationId, long messageId, int mentionedByUserId, CancellationToken cancellationToken = default);
}
