using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

public class CleanupJob
{
    private static readonly TimeSpan NotificationRetentionPeriod = TimeSpan.FromDays(90);
    private static readonly TimeSpan ReportingEventRetentionPeriod = TimeSpan.FromDays(365);

    private readonly IRepository<Notification> _notificationRepository;
    private readonly IRepository<ReportingEvent> _reportingEventRepository;
    private readonly IRepository<Attachment> _attachmentRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CleanupJob> _logger;

    public CleanupJob(
        IRepository<Notification> notificationRepository,
        IRepository<ReportingEvent> reportingEventRepository,
        IRepository<Attachment> attachmentRepository,
        IRepository<Message> messageRepository,
        IUnitOfWork unitOfWork,
        ILogger<CleanupJob> logger)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _reportingEventRepository = reportingEventRepository ?? throw new ArgumentNullException(nameof(reportingEventRepository));
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Cleans up old notifications, reporting events, and orphaned attachments.
    /// Intended to be scheduled by Hangfire as a weekly recurring job.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting cleanup job");

        var notificationsDeleted = await CleanupOldNotificationsAsync(cancellationToken);
        var reportingEventsDeleted = await CleanupOldReportingEventsAsync(cancellationToken);
        var attachmentsDeleted = await CleanupOrphanedAttachmentsAsync(cancellationToken);

        _logger.LogInformation(
            "Cleanup job completed. Notifications deleted: {Notifications}, " +
            "Reporting events deleted: {ReportingEvents}, Orphaned attachments deleted: {Attachments}",
            notificationsDeleted, reportingEventsDeleted, attachmentsDeleted);
    }

    private async Task<int> CleanupOldNotificationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cutoff = DateTime.UtcNow.Subtract(NotificationRetentionPeriod);

            var oldNotifications = await _notificationRepository.FindAsync(
                n => n.ReadAt != null && n.CreatedAt < cutoff,
                cancellationToken);

            foreach (var notification in oldNotifications)
            {
                await _notificationRepository.DeleteAsync(notification, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cleaned up {Count} old read notifications", oldNotifications.Count);
            return oldNotifications.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean up old notifications");
            return 0;
        }
    }

    private async Task<int> CleanupOldReportingEventsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cutoff = DateTime.UtcNow.Subtract(ReportingEventRetentionPeriod);

            var oldEvents = await _reportingEventRepository.FindAsync(
                e => e.CreatedAt < cutoff,
                cancellationToken);

            foreach (var reportingEvent in oldEvents)
            {
                await _reportingEventRepository.DeleteAsync(reportingEvent, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cleaned up {Count} old reporting events", oldEvents.Count);
            return oldEvents.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean up old reporting events");
            return 0;
        }
    }

    private async Task<int> CleanupOrphanedAttachmentsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Find attachments whose parent message no longer exists
            var allAttachments = await _attachmentRepository.GetAllAsync(cancellationToken);
            var orphanedCount = 0;

            foreach (var attachment in allAttachments)
            {
                var message = await _messageRepository.GetByIdAsync(attachment.MessageId, cancellationToken);
                if (message is null)
                {
                    await _attachmentRepository.DeleteAsync(attachment, cancellationToken);
                    orphanedCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cleaned up {Count} orphaned attachments", orphanedCount);
            return orphanedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean up orphaned attachments");
            return 0;
        }
    }
}
